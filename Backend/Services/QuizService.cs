using System.Collections.Concurrent;
using System.Data;
using System.Text.Json;
using Backend.Configuration;
using Backend.Exceptions;
using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class QuizService(ServiceContext<QuizService> ctx) : IQuizService {
    private static readonly ConcurrentDictionary<(uint UserId, uint QuizId), SemaphoreSlim> AttemptLocks = new();

    public async Task<(QuestionResponseDto[] Questions, Metadata Metadata)> GetQuestionsAsync(uint quizId, RequestQuery requestQuery, CancellationToken ct)
    {
        var quizInfo = await ctx.Db.Quizzes.AsNoTracking()
                           .Where(q => q.Id == quizId)
                           .Select(q => new {
                               q.Section.CourseId,
                               q.RandomizeQuestions
                           })
                           .FirstOrDefaultAsync(ct)
                       ?? throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(ctx.Db, quizInfo.CourseId, ctx.AuthUserId, ct);

        var query = ctx.Db.Questions.AsNoTracking()
            .Where(question => question.QuizId == quizId);

        query = quizInfo.RandomizeQuestions ? query.OrderBy(q => q.Id * 2654435761u ^ ctx.AuthUserId) : query.OrderBy(q => q.Id);

        var (questions, metadata) = await query.Select(question => new QuestionResponseDto(
                question.Id,
                question.Text,
                question.ImageUrl,
                question.Type,
                question.Points,
                question.QuizId))
            .ToPagedResultAsync(requestQuery, ctx.MaxUnpagedItems, ct);

        return (questions, new Metadata(metadata, requestQuery));
    }

    public async Task<uint> CreateAsync(CreateQuizRequestDto request, CancellationToken ct)
    {
        await EnsureCanManageSectionAsync(request.SectionId, ct);
        Quiz quiz = new() {
            Title = request.Title,
            SectionId = request.SectionId,
            MaxAttempts = request.MaxAttempts,
            TimeLimitSeconds = request.TimeLimitSeconds,
            PassingThresholdPercent = request.PassingThresholdPercent,
            RandomizeQuestions = request.RandomizeQuestions,
            RandomizeAnswers = request.RandomizeAnswers
        };
        ctx.Db.Quizzes.Add(quiz);
        await ctx.Db.SaveChangesAsync(ct);
        return quiz.Id;
    }

    public async Task UpdateAsync(uint quizId, CreateQuizRequestDto request, CancellationToken ct)
    {
        var quiz = await ctx.Db.Quizzes.FirstOrDefaultAsync(candidate => candidate.Id == quizId, ct);
        if (quiz is null)
            throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        await EnsureCanManageSectionAsync(quiz.SectionId, ct);
        if (quiz.SectionId != request.SectionId)
            await EnsureCanManageSectionAsync(request.SectionId, ct);

        quiz.Title = request.Title;
        quiz.SectionId = request.SectionId;
        quiz.MaxAttempts = request.MaxAttempts;
        quiz.TimeLimitSeconds = request.TimeLimitSeconds;
        quiz.PassingThresholdPercent = request.PassingThresholdPercent;
        quiz.RandomizeQuestions = request.RandomizeQuestions;
        quiz.RandomizeAnswers = request.RandomizeAnswers;
        await ctx.Db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(uint quizId, CancellationToken ct)
    {
        await EnsureCanManageQuizAsync(quizId, ct);
        await ctx.Db.Quizzes.Where(quiz => quiz.Id == quizId).ExecuteDeleteAsync(ct);
    }

    public async Task<QuizResponseDto> GetForEditorAsync(uint quizId, CancellationToken ct)
    {
        await EnsureCanManageQuizAsync(quizId, ct);
        return await GetByIdAsync(quizId, ct);
    }

    public async Task<(MyQuizAttemptResponseDto[] Attempts, Metadata Metadata)> GetMyAttemptsAsync(uint quizId, RequestQuery request, CancellationToken ct)
    {
        if (!await ctx.Db.Quizzes.AsNoTracking().AnyAsync(quiz => quiz.Id == quizId, ct))
            throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        var (attempts, metadata) = await ctx.Db.QuizAttempts.AsNoTracking()
            .Where(attempt => attempt.QuizId == quizId && attempt.StudentId == ctx.AuthUserId)
            .OrderByDescending(attempt => attempt.StartedAtUtc)
            .Select(attempt => new MyQuizAttemptResponseDto(
                attempt.Id,
                attempt.StartedAtUtc,
                attempt.SubmittedAtUtc,
                attempt.ScorePercent,
                attempt.IsPassed))
            .ToPagedResultAsync(request, ctx.MaxUnpagedItems, ct);

        return (attempts, new Metadata(metadata, request));
    }

    public async Task<QuizAttemptDetailResponseDto> GetAttemptByAttemptIdAsync(uint attemptId, CancellationToken ct)
    {
        var attempt = await ctx.Db.QuizAttempts.AsNoTracking()
            .Where(candidate => candidate.Id == attemptId)
            .Select(candidate => new {
                candidate.Id,
                candidate.QuizId,
                candidate.StudentId,
                candidate.StartedAtUtc,
                candidate.SubmittedAtUtc,
                candidate.ScorePercent,
                candidate.IsPassed,
                candidate.Quiz.Section.CourseId
            })
            .FirstOrDefaultAsync(ct);

        var answers = (await ctx.Db.StudentAnswers.AsNoTracking()
                .Where(candidate => candidate.QuizAttemptId == attemptId)
                .Select(candidate => new {
                    candidate.QuestionId,
                    candidate.SelectedOptionIds
                })
                .ToListAsync(ct))
            .ToDictionary(x => x.QuestionId, x => x.SelectedOptionIds);

        if (attempt is null)
            throw ServiceException.NotFound(Resources.ErrorQuizAttemptNotFound);

        if (attempt.StudentId != ctx.AuthUserId)
            await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, attempt.CourseId, ctx.AuthUserId, ct);

        return new QuizAttemptDetailResponseDto(
            attempt.Id,
            attempt.QuizId,
            attempt.StudentId,
            attempt.StartedAtUtc,
            attempt.SubmittedAtUtc,
            attempt.ScorePercent,
            attempt.IsPassed,
            answers);
    }

    public async Task<uint> StartAttemptAsync(StartAttemptRequestDto request, CancellationToken ct)
    {
        var quiz = await ctx.Db.Quizzes.AsNoTracking()
                       .Where(candidate => candidate.Id == request.QuizId)
                       .Select(candidate => new {
                           candidate.Id,
                           candidate.MaxAttempts,
                           candidate.TimeLimitSeconds,
                           candidate.Section.CourseId
                       })
                       .FirstOrDefaultAsync(ct)
                   ?? throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        var role = await ctx.Db.Users.AsNoTracking()
                       .Where(user => user.Id == ctx.AuthUserId)
                       .Select(user => (UserRole?)user.Role)
                       .FirstOrDefaultAsync(ct)
                   ?? throw ServiceException.NotFound(Resources.ErrorUserNotFound);
        if (role != UserRole.Student)
            throw ServiceException.Forbidden(Resources.ErrorOnlyStudentsCanStartQuiz);

        bool isEnrolled = await ctx.Db.Enrollments.AsNoTracking()
            .AnyAsync(enrollment => enrollment.StudentId == ctx.AuthUserId && enrollment.CourseId == quiz.CourseId, ct);
        if (!isEnrolled)
            throw ServiceException.Forbidden(Resources.ErrorQuizEnrollmentRequired);

        var attemptLock = AttemptLocks.GetOrAdd((ctx.AuthUserId, quiz.Id), _ => new SemaphoreSlim(1, 1));
        await attemptLock.WaitAsync(ct);
        try
        {
            await using var transaction = await ctx.Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

            var nowUtc = DateTime.UtcNow;
            var unsubmittedAttempts = await ctx.Db.QuizAttempts
                .Where(a => a.QuizId == quiz.Id && a.StudentId == ctx.AuthUserId && a.SubmittedAtUtc == null)
                .ToListAsync(ct);

            foreach (var openAttempt in unsubmittedAttempts)
            {
                if (!IsAttemptExpired(openAttempt.StartedAtUtc, quiz.TimeLimitSeconds, nowUtc, ctx.Cfg.Value))
                    throw ServiceException.Conflict(Resources.ErrorActiveAttempt);

                CloseAsExpired(openAttempt, nowUtc);
            }

            int attemptsCount = await ctx.Db.QuizAttempts.AsNoTracking()
                .CountAsync(attempt => attempt.QuizId == quiz.Id && attempt.StudentId == ctx.AuthUserId, ct);

            if (quiz.MaxAttempts.HasValue && attemptsCount >= quiz.MaxAttempts.Value)
            {
                await ctx.Db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                throw ServiceException.Conflict(Resources.ErrorQuizMaxAttemptsReached);
            }

            QuizAttempt attempt = new() { QuizId = quiz.Id, StudentId = ctx.AuthUserId };
            ctx.Db.QuizAttempts.Add(attempt);
            await ctx.Db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return attempt.Id;
        }
        finally
        {
            attemptLock.Release();
        }
    }

    public async Task<QuizAttemptResponseDto> EndAttemptAsync(EndAttemptRequestDto request, CancellationToken ct)
    {
        var attemptMeta = await ctx.Db.QuizAttempts.AsNoTracking()
                              .Where(a => a.Id == request.AttemptId && a.StudentId == ctx.AuthUserId)
                              .Select(a => new { a.Id, a.QuizId })
                              .FirstOrDefaultAsync(ct)
                          ?? throw ServiceException.NotFound(Resources.ErrorQuizAttemptNotFound);

        var attemptLock = AttemptLocks.GetOrAdd((ctx.AuthUserId, attemptMeta.QuizId), _ => new SemaphoreSlim(1, 1));
        await attemptLock.WaitAsync(ct);
        try
        {
            var attempt = await ctx.Db.QuizAttempts.Include(a => a.Quiz)
                              .FirstOrDefaultAsync(a => a.Id == request.AttemptId && a.StudentId == ctx.AuthUserId, ct)
                          ?? throw ServiceException.NotFound(Resources.ErrorQuizAttemptNotFound);

            if (attempt.SubmittedAtUtc.HasValue)
                throw ServiceException.Conflict(Resources.ErrorAttemptAlreadySubmitted);

            var nowUtc = DateTime.UtcNow;
            if (IsAttemptExpired(attempt.StartedAtUtc, attempt.Quiz.TimeLimitSeconds, nowUtc, ctx.Cfg.Value))
            {
                CloseAsExpired(attempt, nowUtc);
                await ctx.Db.SaveChangesAsync(ct);

                throw ServiceException.Forbidden(Resources.ErrorAttemptTimeLimitReached);
            }

            var submittedAnswers = ParseSubmittedAnswers(request.AnswersJson);

            var questions = await ctx.Db.Questions.AsNoTracking()
                .Where(q => q.QuizId == attempt.QuizId)
                .Include(q => q.AnswerOptions)
                .ToListAsync(ct);

            var questionIds = questions.Select(q => q.Id).ToHashSet();
            if (submittedAnswers.Any(answer => !questionIds.Contains(answer.Key) || answer.Value is null))
                throw ServiceException.BadRequest(Resources.ErrorAnswersContainUnknownQuestion);

            int totalPoints = 0;
            int awardedPoints = 0;

            foreach (var question in questions)
            {
                uint[] selectedIds = submittedAnswers.GetValueOrDefault(question.Id, []);
                if (selectedIds.Distinct().Count() != selectedIds.Length)
                    throw ServiceException.BadRequest(Resources.ErrorAnswersContainDuplicateOptions);

                var validOptionIds = question.AnswerOptions.Select(o => o.Id).ToHashSet();
                if (selectedIds.Any(optionId => !validOptionIds.Contains(optionId)))
                    throw ServiceException.BadRequest(Resources.ErrorAnswerOptionDoesNotBelongToQuestion);

                uint[] correctIds = [.. question.AnswerOptions.Where(o => o.IsCorrect).Select(o => o.Id).Order()];
                uint[] selectedSorted = [.. selectedIds.Order()];

                bool isCorrect = correctIds.Length > 0 && correctIds.SequenceEqual(selectedSorted);
                ushort questionAwardedPoints = isCorrect ? question.Points : (ushort)0;

                totalPoints += question.Points;
                awardedPoints += questionAwardedPoints;

                ctx.Db.StudentAnswers.Add(new StudentAnswer {
                    QuizAttemptId = attempt.Id,
                    QuestionId = question.Id,
                    SelectedOptionIds = selectedSorted,
                    IsFullyCorrect = isCorrect,
                    AwardedPoints = questionAwardedPoints
                });
            }

            attempt.SubmittedAtUtc = nowUtc;
            attempt.ScorePercent = totalPoints == 0 ? 0m : (decimal)awardedPoints * 100 / totalPoints;
            attempt.IsPassed = attempt.ScorePercent >= attempt.Quiz.PassingThresholdPercent;
            await ctx.Db.SaveChangesAsync(ct);

            return MapToResponseDto(attempt);
        }
        finally
        {
            attemptLock.Release();
        }
    }

    public async Task<(QuizAttemptResponseDto[] Attempts, Metadata Metadata)> GetAttemptsAsync(uint quizId, RequestQuery request, CancellationToken ct)
    {
        await EnsureCanManageQuizAsync(quizId, ct);

        var (attempts, metadata) = await ctx.Db.QuizAttempts.AsNoTracking()
            .Where(attempt => attempt.QuizId == quizId)
            .OrderByDescending(attempt => attempt.StartedAtUtc)
            .Select(attempt => new QuizAttemptResponseDto(
                attempt.Id,
                attempt.QuizId,
                attempt.StudentId,
                attempt.StartedAtUtc,
                attempt.SubmittedAtUtc,
                attempt.ScorePercent,
                attempt.IsPassed))
            .ToPagedResultAsync(request, ctx.MaxUnpagedItems, ct);

        return (attempts, new Metadata(metadata, request));
    }

    public async Task<QuizResponseDto> GetByIdAsync(uint quizId, CancellationToken ct)
    {
        var result = await ctx.Db.Quizzes.AsNoTracking()
                         .Where(quiz => quiz.Id == quizId)
                         .Select(quiz => new {
                             Dto = new QuizResponseDto(
                                 quiz.Id,
                                 quiz.Title,
                                 quiz.MaxAttempts,
                                 quiz.TimeLimitSeconds,
                                 quiz.PassingThresholdPercent,
                                 quiz.SectionId),
                             quiz.Section.CourseId
                         })
                         .FirstOrDefaultAsync(ct)
                     ?? throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(ctx.Db, result.CourseId, ctx.AuthUserId, ct);

        return result.Dto;
    }

    private static Dictionary<uint, uint[]> ParseSubmittedAnswers(string answersJson)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<uint, uint[]>>(answersJson) ?? throw ServiceException.BadRequest(Resources.ErrorAnswersMustBeJsonObject);
        }
        catch (JsonException)
        {
            throw ServiceException.BadRequest(Resources.ErrorAnswersMustBeValidJsonObject);
        }
    }

    private static void CloseAsExpired(QuizAttempt attempt, DateTime closedAtUtc)
    {
        attempt.SubmittedAtUtc = closedAtUtc;
        attempt.ScorePercent = 0m;
        attempt.IsPassed = false;
    }

    private static bool IsAttemptExpired(DateTime startedAtUtc, uint? timeLimitSeconds, DateTime nowUtc, Config cfg) => timeLimitSeconds.HasValue && startedAtUtc.AddSeconds(timeLimitSeconds.Value + cfg.NetworkDelayToleranceSeconds) <= nowUtc;

    private static QuizAttemptResponseDto MapToResponseDto(QuizAttempt attempt) => new(attempt.Id, attempt.QuizId, attempt.StudentId, attempt.StartedAtUtc, attempt.SubmittedAtUtc, attempt.ScorePercent, attempt.IsPassed);


    private async Task EnsureCanManageQuizAsync(uint quizId, CancellationToken ct) => await EnsureCanManageQuizAsync(ctx.AuthUserId, quizId, ct);

    private async Task EnsureCanManageQuizAsync(uint userId, uint quizId, CancellationToken ct)
    {
        uint courseId = await ctx.Db.Quizzes.AsNoTracking()
                            .Where(quiz => quiz.Id == quizId)
                            .Select(quiz => (uint?)quiz.Section.CourseId)
                            .FirstOrDefaultAsync(ct)
                        ?? throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, courseId, userId, ct);
    }

    private async Task EnsureCanManageSectionAsync(uint sectionId, CancellationToken ct) => await EnsureCanManageSectionAsync(ctx.AuthUserId, sectionId, ct);

    private async Task EnsureCanManageSectionAsync(uint userId, uint sectionId, CancellationToken ct)
    {
        uint courseId = await ctx.Db.Sections.AsNoTracking()
                            .Where(section => section.Id == sectionId)
                            .Select(section => (uint?)section.CourseId)
                            .FirstOrDefaultAsync(ct)
                        ?? throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, courseId, userId, ct);
    }
}