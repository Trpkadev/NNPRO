using System.Data;
using System.Text.Json;
using Backend.Exceptions;
using Backend.Extensions;
using Backend.Logging;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class QuizService(ServiceContext<QuizService> ctx) : IQuizService {
    private static readonly SemaphoreSlim StartAttemptLock = new(1);

    public async Task<(QuestionResponseDto[] Questions, Metadata Metadata)> GetQuestionsAsync(uint quizId, uint userId, RequestQuery requestQuery, CancellationToken ct)
    {
        var quizInfo = await ctx.Db.Quizzes.AsNoTracking()
                           .Where(q => q.Id == quizId)
                           .Select(q => new {
                               q.Section.CourseId,
                               q.RandomizeQuestions
                           })
                           .FirstOrDefaultAsync(ct)
                       ?? throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(ctx.Db, quizInfo.CourseId, userId, ct);

        var query = ctx.Db.Questions.AsNoTracking()
            .Where(question => question.QuizId == quizId);

        query = quizInfo.RandomizeQuestions ? query.OrderBy(q => q.Id * 2654435761u ^ userId) : query.OrderBy(q => q.Id);

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

    public async Task<uint> CreateAsync(uint requestingUserId, CreateQuizRequestDto request, CancellationToken ct)
    {
        await EnsureCanManageSectionAsync(requestingUserId, request.SectionId, ct);
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

        ctx.Logger.QuizCreated(requestingUserId, quiz.Id, request.SectionId);
        return quiz.Id;
    }

    public async Task UpdateAsync(uint requestingUserId, uint quizId, CreateQuizRequestDto request, CancellationToken ct)
    {
        var quiz = await ctx.Db.Quizzes.FirstOrDefaultAsync(candidate => candidate.Id == quizId, ct);
        if (quiz is null)
            throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        await EnsureCanManageSectionAsync(requestingUserId, quiz.SectionId, ct);
        if (quiz.SectionId != request.SectionId)
            await EnsureCanManageSectionAsync(requestingUserId, request.SectionId, ct);

        quiz.Title = request.Title;
        quiz.SectionId = request.SectionId;
        quiz.MaxAttempts = request.MaxAttempts;
        quiz.TimeLimitSeconds = request.TimeLimitSeconds;
        quiz.PassingThresholdPercent = request.PassingThresholdPercent;
        quiz.RandomizeQuestions = request.RandomizeQuestions;
        quiz.RandomizeAnswers = request.RandomizeAnswers;
        await ctx.Db.SaveChangesAsync(ct);

        ctx.Logger.QuizUpdated(requestingUserId, quizId, request.SectionId);
    }

    public async Task DeleteAsync(uint requestingUserId, uint quizId, CancellationToken ct)
    {
        await EnsureCanManageQuizAsync(requestingUserId, quizId, ct);
        await ctx.Db.Quizzes.Where(quiz => quiz.Id == quizId).ExecuteDeleteAsync(ct);

        ctx.Logger.QuizDeleted(requestingUserId, quizId);
    }

    public async Task<QuizResponseDto> GetForEditorAsync(uint requestingUserId, uint quizId, CancellationToken ct)
    {
        await EnsureCanManageQuizAsync(requestingUserId, quizId, ct);
        return await GetByIdAsync(quizId, requestingUserId, ct);
    }

    public async Task<(MyQuizAttemptResponseDto[] Attempts, Metadata Metadata)> GetMyAttemptsAsync(uint studentId, uint quizId, RequestQuery request, CancellationToken ct)
    {
        if (!await ctx.Db.Quizzes.AsNoTracking().AnyAsync(quiz => quiz.Id == quizId, ct))
            throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        var (attempts, metadata) = await ctx.Db.QuizAttempts.AsNoTracking()
            .Where(attempt => attempt.QuizId == quizId && attempt.StudentId == studentId)
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

    public async Task<QuizAttemptResponseDto> GetAttemptByAttemptIdAsync(uint requestingUserId, uint attemptId, CancellationToken ct)
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
        if (attempt is null)
            throw ServiceException.NotFound(Resources.ErrorQuizAttemptNotFound);

        if (attempt.StudentId != requestingUserId)
            await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, requestingUserId, attempt.CourseId, ct);

        return new QuizAttemptResponseDto(
            attempt.Id,
            attempt.QuizId,
            attempt.StudentId,
            attempt.StartedAtUtc,
            attempt.SubmittedAtUtc,
            attempt.ScorePercent,
            attempt.IsPassed);
    }

    public async Task<uint> StartAttemptAsync(uint studentId, StartAttemptRequestDto request, CancellationToken ct)
    {
        await StartAttemptLock.WaitAsync(ct);
        try
        {
            var quiz = await ctx.Db.Quizzes.AsNoTracking()
                .Where(candidate => candidate.Id == request.QuizId)
                .Select(candidate => new {
                    candidate.Id,
                    candidate.MaxAttempts,
                    candidate.TimeLimitSeconds,
                    candidate.Section.CourseId
                })
                .FirstOrDefaultAsync(ct);
            if (quiz is null)
                throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

            var role = await ctx.Db.Users.AsNoTracking()
                .Where(user => user.Id == studentId)
                .Select(user => (UserRole?)user.Role)
                .FirstOrDefaultAsync(ct);
            if (!role.HasValue)
                throw ServiceException.NotFound(Resources.ErrorUserNotFound);
            if (role != UserRole.Student)
                throw ServiceException.Forbidden(Resources.ErrorOnlyStudentsCanStartQuiz);

            await using var transaction = await ctx.Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

            var nowUtc = DateTime.UtcNow;
            var unsubmittedAttempts = await ctx.Db.QuizAttempts.Where(a => a.QuizId == quiz.Id && a.StudentId == studentId && a.SubmittedAtUtc == null)
                .ToListAsync(ct);

            foreach (var openAttempt in unsubmittedAttempts)
            {
                bool isExpired = quiz.TimeLimitSeconds.HasValue && openAttempt.StartedAtUtc.AddSeconds(quiz.TimeLimitSeconds.Value) <= nowUtc;

                if (!isExpired)
                {
                    ctx.Logger.ActiveAttemptConflict(studentId, quiz.Id);
                    throw ServiceException.Conflict(Resources.ErrorActiveAttempt);
                }

                openAttempt.SubmittedAtUtc = nowUtc;
                openAttempt.ScorePercent = 0m;
                openAttempt.IsPassed = false;
                ctx.Logger.ExpiredAttemptAutoClosed(openAttempt.Id, studentId, quiz.Id);
            }

            bool isEnrolled = await ctx.Db.Enrollments.AsNoTracking()
                .AnyAsync(enrollment => enrollment.StudentId == studentId && enrollment.CourseId == quiz.CourseId, ct);
            if (!isEnrolled)
                throw ServiceException.Forbidden(Resources.ErrorQuizEnrollmentRequired);

            int attemptsCount = await ctx.Db.QuizAttempts.AsNoTracking()
                .CountAsync(attempt => attempt.QuizId == quiz.Id && attempt.StudentId == studentId, ct);
            if (quiz.MaxAttempts.HasValue && attemptsCount >= quiz.MaxAttempts.Value)
            {
                if (unsubmittedAttempts.Count > 0)
                {
                    await ctx.Db.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                }

                ctx.Logger.MaxAttemptsReached(studentId, quiz.Id, quiz.MaxAttempts.Value);
                throw ServiceException.Conflict(Resources.ErrorQuizMaxAttemptsReached);
            }

            QuizAttempt attempt = new() { QuizId = quiz.Id, StudentId = studentId };
            ctx.Db.QuizAttempts.Add(attempt);
            await ctx.Db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            ctx.Logger.AttemptStarted(studentId, attempt.Id, quiz.Id);
            return attempt.Id;
        }
        finally
        {
            StartAttemptLock.Release();
        }
    }

    public async Task<QuizAttemptResponseDto> EndAttemptAsync(uint studentId, EndAttemptRequestDto request, CancellationToken ct)
    {
        using var scope = ctx.Logger.BeginScope(new Dictionary<string, object> {
            ["AttemptId"] = request.AttemptId,
            ["StudentId"] = studentId
        });

        var attempt = await ctx.Db.QuizAttempts.Include(attempt => attempt.Quiz)
                          .ThenInclude(quiz => quiz.Questions)
                          .ThenInclude(question => question.AnswerOptions)
                          .FirstOrDefaultAsync(attempt => attempt.Id == request.AttemptId && attempt.StudentId == studentId, ct)
                      ?? throw ServiceException.NotFound(Resources.ErrorQuizAttemptNotFound);

        if (attempt.SubmittedAtUtc.HasValue)
            throw ServiceException.Conflict(Resources.ErrorAttemptAlreadySubmitted);

        if (attempt.Quiz.TimeLimitSeconds.HasValue && attempt.StartedAtUtc.AddSeconds(attempt.Quiz.TimeLimitSeconds.Value) < DateTime.UtcNow)
        {
            ctx.Logger.AttemptTimeLimitExceeded(attempt.Id, studentId, attempt.QuizId);
            throw ServiceException.Forbidden(Resources.ErrorAttemptTimeLimitReached);
        }

        Dictionary<uint, uint[]> submittedAnswers;
        try
        {
            submittedAnswers = JsonSerializer.Deserialize<Dictionary<uint, uint[]>>(request.AnswersJson) ?? throw ServiceException.BadRequest(Resources.ErrorAnswersMustBeJsonObject);
        }
        catch (JsonException)
        {
            ctx.Logger.InvalidAttemptSubmission(attempt.Id, studentId, "MalformedJson");
            throw ServiceException.BadRequest(Resources.ErrorAnswersMustBeValidJsonObject);
        }
        catch (ServiceException)
        {
            ctx.Logger.InvalidAttemptSubmission(attempt.Id, studentId, "NullJsonObject");
            throw;
        }

        var questionById = attempt.Quiz.Questions.ToDictionary(question => question.Id);
        if (submittedAnswers.Any(answer => !questionById.ContainsKey(answer.Key) || answer.Value is null))
        {
            ctx.Logger.InvalidAttemptSubmission(attempt.Id, studentId, "UnknownQuestionOrNullAnswers");
            throw ServiceException.BadRequest(Resources.ErrorAnswersContainUnknownQuestion);
        }

        int totalPoints = 0;
        int awardedPoints = 0;
        foreach (var question in attempt.Quiz.Questions)
        {
            uint[] selectedIds = submittedAnswers.GetValueOrDefault(question.Id, []);
            if (selectedIds.Distinct().Count() != selectedIds.Length)
            {
                ctx.Logger.InvalidAttemptSubmission(attempt.Id, studentId, "DuplicateOptions");
                throw ServiceException.BadRequest(Resources.ErrorAnswersContainDuplicateOptions);
            }

            uint[] correctIds = [
                .. question.AnswerOptions.Where(option => option.IsCorrect)
                    .Select(option => option.Id)
                    .Order()
            ];
            uint[] selectedSorted = [.. selectedIds.Order()];
            if (selectedSorted.Any(optionId => question.AnswerOptions.All(option => option.Id != optionId)))
            {
                ctx.Logger.InvalidAttemptSubmission(attempt.Id, studentId, "OptionDoesNotBelongToQuestion");
                throw ServiceException.BadRequest(Resources.ErrorAnswerOptionDoesNotBelongToQuestion);
            }

            bool isCorrect = correctIds.Length > 0 && correctIds.SequenceEqual(selectedSorted);
            ushort questionAwardedPoints = isCorrect ? question.Points : (ushort)0;
            totalPoints += question.Points;
            awardedPoints += questionAwardedPoints;
            ctx.Db.StudentAnswers.Add(new StudentAnswer {
                QuizAttemptId = attempt.Id,
                QuestionId = question.Id,
                SelectedOptionIdsJson = JsonSerializer.Serialize(selectedSorted),
                IsFullyCorrect = isCorrect,
                AwardedPoints = questionAwardedPoints
            });
        }

        attempt.SubmittedAtUtc = DateTime.UtcNow;
        attempt.ScorePercent = totalPoints == 0 ? 0 : (decimal)awardedPoints * 100 / totalPoints;
        attempt.IsPassed = attempt.ScorePercent >= attempt.Quiz.PassingThresholdPercent;
        await ctx.Db.SaveChangesAsync(ct);

        ctx.Logger.AttemptSubmitted(studentId, attempt.Id, attempt.QuizId, attempt.ScorePercent, attempt.IsPassed);

        return new QuizAttemptResponseDto(attempt.Id, attempt.QuizId, attempt.StudentId, attempt.StartedAtUtc, attempt.SubmittedAtUtc, attempt.ScorePercent, attempt.IsPassed);
    }

    public async Task<(QuizAttemptResponseDto[] Attempts, Metadata Metadata)> GetAttemptsAsync(uint requestingUserId, uint quizId, RequestQuery request, CancellationToken ct)
    {
        await EnsureCanManageQuizAsync(requestingUserId, quizId, ct);

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

    public async Task<QuizResponseDto> GetByIdAsync(uint quizId, uint userId, CancellationToken ct)
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

        await CourseAuthorization.EnsureCanAccessCourseAsync(ctx.Db, result.CourseId, userId, ct);

        return result.Dto;
    }

    private async Task EnsureCanManageQuizAsync(uint requestingUserId, uint quizId, CancellationToken ct)
    {
        uint? courseId = await ctx.Db.Quizzes.AsNoTracking()
            .Where(quiz => quiz.Id == quizId)
            .Select(quiz => (uint?)quiz.Section.CourseId)
            .FirstOrDefaultAsync(ct);
        if (!courseId.HasValue)
            throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, requestingUserId, courseId.Value, ct);
    }

    private async Task EnsureCanManageSectionAsync(uint requestingUserId, uint sectionId, CancellationToken ct)
    {
        uint? courseId = await ctx.Db.Sections.AsNoTracking()
            .Where(section => section.Id == sectionId)
            .Select(section => (uint?)section.CourseId)
            .FirstOrDefaultAsync(ct);
        if (!courseId.HasValue)
            throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, requestingUserId, courseId.Value, ct);
    }
}