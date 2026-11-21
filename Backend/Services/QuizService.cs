using System.Collections.Concurrent;
using System.Data;
using System.Text.Json;
using Backend.Configuration;
using Backend.Data;
using Backend.Exceptions;
using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class QuizService(AppDbContext dbContext, IOptions<DatabaseLimitsOptions> dbLimitsOptions, IOptions<Config> cfg) : IQuizService {
    private static readonly ConcurrentDictionary<(uint StudentId, uint QuizId), SemaphoreSlim> AttemptLocks = new();

    public async Task<(QuestionResponseDto[] Questions, Metadata Metadata)> GetQuestionsAsync(uint quizId, uint userId, RequestQuery requestQuery, CancellationToken ct)
    {
        var quizInfo = await dbContext.Quizzes.AsNoTracking()
                           .Where(q => q.Id == quizId)
                           .Select(q => new {
                               q.Section.CourseId,
                               q.RandomizeQuestions
                           })
                           .FirstOrDefaultAsync(ct)
                       ?? throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(dbContext, quizInfo.CourseId, userId, ct);

        var query = dbContext.Questions.AsNoTracking()
            .Where(question => question.QuizId == quizId);

        query = quizInfo.RandomizeQuestions ? query.OrderBy(q => q.Id * 2654435761u ^ userId) : query.OrderBy(q => q.Id);

        var (questions, metadata) = await query.Select(question => new QuestionResponseDto(
                question.Id,
                question.Text,
                question.ImageUrl,
                question.Type,
                question.Points,
                question.QuizId))
            .ToPagedResultAsync(requestQuery, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

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
        dbContext.Quizzes.Add(quiz);
        await dbContext.SaveChangesAsync(ct);
        return quiz.Id;
    }

    public async Task UpdateAsync(uint requestingUserId, uint quizId, CreateQuizRequestDto request, CancellationToken ct)
    {
        var quiz = await dbContext.Quizzes.FirstOrDefaultAsync(candidate => candidate.Id == quizId, ct);
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
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(uint requestingUserId, uint quizId, CancellationToken ct)
    {
        await EnsureCanManageQuizAsync(requestingUserId, quizId, ct);
        await dbContext.Quizzes.Where(quiz => quiz.Id == quizId).ExecuteDeleteAsync(ct);
    }

    public async Task<QuizResponseDto> GetForEditorAsync(uint requestingUserId, uint quizId, CancellationToken ct)
    {
        await EnsureCanManageQuizAsync(requestingUserId, quizId, ct);
        return await GetByIdAsync(quizId, requestingUserId, ct);
    }

    public async Task<(MyQuizAttemptResponseDto[] Attempts, Metadata Metadata)> GetMyAttemptsAsync(uint studentId, uint quizId, RequestQuery request, CancellationToken ct)
    {
        if (!await dbContext.Quizzes.AsNoTracking().AnyAsync(quiz => quiz.Id == quizId, ct))
            throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        var (attempts, metadata) = await dbContext.QuizAttempts.AsNoTracking()
            .Where(attempt => attempt.QuizId == quizId && attempt.StudentId == studentId)
            .OrderByDescending(attempt => attempt.StartedAtUtc)
            .Select(attempt => new MyQuizAttemptResponseDto(
                attempt.Id,
                attempt.StartedAtUtc,
                attempt.SubmittedAtUtc,
                attempt.ScorePercent,
                attempt.IsPassed))
            .ToPagedResultAsync(request, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        return (attempts, new Metadata(metadata, request));
    }

    public async Task<QuizAttemptResponseDto> GetAttemptByAttemptIdAsync(uint requestingUserId, uint attemptId, CancellationToken ct)
    {
        var attempt = await dbContext.QuizAttempts.AsNoTracking()
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
            await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, attempt.CourseId, ct);

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
        var quiz = await dbContext.Quizzes.AsNoTracking()
                       .Where(candidate => candidate.Id == request.QuizId)
                       .Select(candidate => new {
                           candidate.Id,
                           candidate.MaxAttempts,
                           candidate.TimeLimitSeconds,
                           candidate.Section.CourseId
                       })
                       .FirstOrDefaultAsync(ct)
                   ?? throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        var role = await dbContext.Users.AsNoTracking()
                       .Where(user => user.Id == studentId)
                       .Select(user => (UserRole?)user.Role)
                       .FirstOrDefaultAsync(ct)
                   ?? throw ServiceException.NotFound(Resources.ErrorUserNotFound);
        if (role != UserRole.Student)
            throw ServiceException.Forbidden(Resources.ErrorOnlyStudentsCanStartQuiz);

        bool isEnrolled = await dbContext.Enrollments.AsNoTracking()
            .AnyAsync(enrollment => enrollment.StudentId == studentId && enrollment.CourseId == quiz.CourseId, ct);
        if (!isEnrolled)
            throw ServiceException.Forbidden(Resources.ErrorQuizEnrollmentRequired);

        var attemptLock = AttemptLocks.GetOrAdd((studentId, quiz.Id), _ => new SemaphoreSlim(1, 1));
        await attemptLock.WaitAsync(ct);
        try
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

            var nowUtc = DateTime.UtcNow;
            var unsubmittedAttempts = await dbContext.QuizAttempts
                .Where(a => a.QuizId == quiz.Id && a.StudentId == studentId && a.SubmittedAtUtc == null)
                .ToListAsync(ct);

            foreach (var openAttempt in unsubmittedAttempts)
            {
                if (!IsAttemptExpired(openAttempt.StartedAtUtc, quiz.TimeLimitSeconds, nowUtc, cfg.Value))
                    throw ServiceException.Conflict(Resources.ErrorActiveAttempt);

                CloseAsExpired(openAttempt, nowUtc);
            }

            int attemptsCount = await dbContext.QuizAttempts.AsNoTracking()
                .CountAsync(attempt => attempt.QuizId == quiz.Id && attempt.StudentId == studentId, ct);

            if (quiz.MaxAttempts.HasValue && attemptsCount >= quiz.MaxAttempts.Value)
            {
                await dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                throw ServiceException.Conflict(Resources.ErrorQuizMaxAttemptsReached);
            }

            QuizAttempt attempt = new() { QuizId = quiz.Id, StudentId = studentId };
            dbContext.QuizAttempts.Add(attempt);
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return attempt.Id;
        }
        finally
        {
            attemptLock.Release();
        }
    }

    public async Task<QuizAttemptResponseDto> EndAttemptAsync(uint studentId, EndAttemptRequestDto request, CancellationToken ct)
    {
        var attemptMeta = await dbContext.QuizAttempts.AsNoTracking()
                              .Where(a => a.Id == request.AttemptId && a.StudentId == studentId)
                              .Select(a => new { a.Id, a.QuizId })
                              .FirstOrDefaultAsync(ct)
                          ?? throw ServiceException.NotFound(Resources.ErrorQuizAttemptNotFound);

        var attemptLock = AttemptLocks.GetOrAdd((studentId, attemptMeta.QuizId), _ => new SemaphoreSlim(1, 1));
        await attemptLock.WaitAsync(ct);
        try
        {
            var attempt = await dbContext.QuizAttempts.Include(a => a.Quiz)
                              .FirstOrDefaultAsync(a => a.Id == request.AttemptId && a.StudentId == studentId, ct)
                          ?? throw ServiceException.NotFound(Resources.ErrorQuizAttemptNotFound);

            if (attempt.SubmittedAtUtc.HasValue)
                throw ServiceException.Conflict(Resources.ErrorAttemptAlreadySubmitted);

            var nowUtc = DateTime.UtcNow;
            if (IsAttemptExpired(attempt.StartedAtUtc, attempt.Quiz.TimeLimitSeconds, nowUtc, cfg.Value))
            {
                CloseAsExpired(attempt, nowUtc);
                await dbContext.SaveChangesAsync(ct);

                throw ServiceException.Forbidden(Resources.ErrorAttemptTimeLimitReached);
            }

            var submittedAnswers = ParseSubmittedAnswers(request.AnswersJson);

            var questions = await dbContext.Questions.AsNoTracking()
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

                dbContext.StudentAnswers.Add(new StudentAnswer {
                    QuizAttemptId = attempt.Id,
                    QuestionId = question.Id,
                    SelectedOptionIdsJson = JsonSerializer.Serialize(selectedSorted),
                    IsFullyCorrect = isCorrect,
                    AwardedPoints = questionAwardedPoints
                });
            }

            attempt.SubmittedAtUtc = nowUtc;
            attempt.ScorePercent = totalPoints == 0 ? 0m : (decimal)awardedPoints * 100 / totalPoints;
            attempt.IsPassed = attempt.ScorePercent >= attempt.Quiz.PassingThresholdPercent;
            await dbContext.SaveChangesAsync(ct);

            return MapToResponseDto(attempt);
        }
        finally
        {
            attemptLock.Release();
        }
    }

    public async Task<(QuizAttemptResponseDto[] Attempts, Metadata Metadata)> GetAttemptsAsync(uint requestingUserId, uint quizId, RequestQuery request, CancellationToken ct)
    {
        await EnsureCanManageQuizAsync(requestingUserId, quizId, ct);

        var (attempts, metadata) = await dbContext.QuizAttempts.AsNoTracking()
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
            .ToPagedResultAsync(request, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        return (attempts, new Metadata(metadata, request));
    }

    public async Task<QuizResponseDto> GetByIdAsync(uint quizId, uint userId, CancellationToken ct)
    {
        var result = await dbContext.Quizzes.AsNoTracking()
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

        await CourseAuthorization.EnsureCanAccessCourseAsync(dbContext, result.CourseId, userId, ct);

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


    private async Task EnsureCanManageQuizAsync(uint requestingUserId, uint quizId, CancellationToken ct)
    {
        uint? courseId = await dbContext.Quizzes.AsNoTracking()
            .Where(quiz => quiz.Id == quizId)
            .Select(quiz => (uint?)quiz.Section.CourseId)
            .FirstOrDefaultAsync(ct);
        if (!courseId.HasValue)
            throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, courseId.Value, ct);
    }

    private async Task EnsureCanManageSectionAsync(uint requestingUserId, uint sectionId, CancellationToken ct)
    {
        uint? courseId = await dbContext.Sections.AsNoTracking()
            .Where(section => section.Id == sectionId)
            .Select(section => (uint?)section.CourseId)
            .FirstOrDefaultAsync(ct);
        if (!courseId.HasValue)
            throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, courseId.Value, ct);
    }
}