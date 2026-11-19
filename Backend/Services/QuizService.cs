using System.Text.Json;
using Backend.Config;
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

public class QuizService(AppDbContext dbContext, IOptions<DatabaseLimitsOptions> dbLimitsOptions) : IQuizService {
    private static readonly SemaphoreSlim StartAttemptLock = new(1);

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

        var (questions, metadata) = await dbContext.Questions.AsNoTracking()
            .Where(question => question.QuizId == quizId)
            .OrderBy(question => question.Id)
            .Select(question => new QuestionResponseDto(
                question.Id,
                question.Text,
                question.ImageUrl,
                question.Type,
                question.Points,
                question.QuizId))
            .ToPagedResultAsync(requestQuery, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        if (quizInfo.RandomizeQuestions)
            Random.Shared.Shuffle(questions);

        return (questions, new Metadata { Pagination = metadata });
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
        await StartAttemptLock.WaitAsync(ct);
        try
        {
            var quiz = await dbContext.Quizzes.AsNoTracking()
                .Where(candidate => candidate.Id == request.QuizId)
                .Select(candidate => new {
                    candidate.Id,
                    candidate.MaxAttempts,
                    candidate.Section.CourseId
                })
                .FirstOrDefaultAsync(ct);
            if (quiz is null)
                throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

            var role = await dbContext.Users.AsNoTracking()
                .Where(user => user.Id == studentId)
                .Select(user => (UserRole?)user.Role)
                .FirstOrDefaultAsync(ct);
            if (!role.HasValue)
                throw ServiceException.NotFound(Resources.ErrorUserNotFound);
            if (role != UserRole.Student)
                throw ServiceException.Forbidden(Resources.ErrorOnlyStudentsCanStartQuiz);

            bool isEnrolled = await dbContext.Enrollments.AsNoTracking()
                .AnyAsync(enrollment => enrollment.StudentId == studentId && enrollment.CourseId == quiz.CourseId, ct);
            if (!isEnrolled)
                throw ServiceException.Forbidden(Resources.ErrorQuizEnrollmentRequired);

            int attemptsCount = await dbContext.QuizAttempts.AsNoTracking()
                .CountAsync(attempt => attempt.QuizId == quiz.Id && attempt.StudentId == studentId, ct);
            if (quiz.MaxAttempts.HasValue && attemptsCount >= quiz.MaxAttempts.Value)
                throw ServiceException.Conflict(Resources.ErrorQuizMaxAttemptsReached);

            QuizAttempt attempt = new() { QuizId = quiz.Id, StudentId = studentId };
            dbContext.QuizAttempts.Add(attempt);
            await dbContext.SaveChangesAsync(ct);
            return attempt.Id;
        }
        finally
        {
            StartAttemptLock.Release();
        }
    }

    public async Task<QuizAttemptResponseDto> EndAttemptAsync(uint studentId, EndAttemptRequestDto request, CancellationToken ct)
    {
        var attempt = await dbContext.QuizAttempts
            .Include(candidate => candidate.Quiz)
            .ThenInclude(quiz => quiz.Questions)
            .ThenInclude(question => question.AnswerOptions)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.AttemptId && candidate.StudentId == studentId, ct);
        if (attempt is null)
            throw ServiceException.NotFound(Resources.ErrorQuizAttemptNotFound);

        if (attempt.SubmittedAtUtc.HasValue)
            throw ServiceException.Conflict(Resources.ErrorAttemptAlreadySubmitted);

        if (attempt.Quiz.TimeLimitSeconds.HasValue && attempt.StartedAtUtc.AddSeconds(attempt.Quiz.TimeLimitSeconds.Value) > DateTime.UtcNow)
            throw ServiceException.Forbidden(Resources.ErrorAttemptTimeLimitReached);

        Dictionary<uint, uint[]> submittedAnswers;
        try
        {
            submittedAnswers = JsonSerializer.Deserialize<Dictionary<uint, uint[]>>(request.AnswersJson)
                               ?? throw ServiceException.BadRequest(Resources.ErrorAnswersMustBeJsonObject);
        }
        catch (JsonException)
        {
            throw ServiceException.BadRequest(Resources.ErrorAnswersMustBeValidJsonObject);
        }

        var questionById = attempt.Quiz.Questions.ToDictionary(question => question.Id);
        if (submittedAnswers.Any(answer => !questionById.ContainsKey(answer.Key) || answer.Value is null))
            throw ServiceException.BadRequest(Resources.ErrorAnswersContainUnknownQuestion);

        int totalPoints = 0;
        int awardedPoints = 0;
        foreach (var question in attempt.Quiz.Questions)
        {
            uint[] selectedIds = submittedAnswers.GetValueOrDefault(question.Id, []);
            if (selectedIds.Distinct().Count() != selectedIds.Length)
                throw ServiceException.BadRequest(Resources.ErrorAnswersContainDuplicateOptions);

            uint[] correctIds = [
                .. question.AnswerOptions.Where(option => option.IsCorrect)
                    .Select(option => option.Id)
                    .Order()
            ];
            uint[] selectedSorted = [.. selectedIds.Order()];
            if (selectedSorted.Any(optionId => question.AnswerOptions.All(option => option.Id != optionId)))
                throw ServiceException.BadRequest(Resources.ErrorAnswerOptionDoesNotBelongToQuestion);

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

        attempt.SubmittedAtUtc = DateTime.UtcNow;
        attempt.ScorePercent = totalPoints == 0 ? 0 : (decimal)awardedPoints * 100 / totalPoints;
        attempt.IsPassed = attempt.ScorePercent >= attempt.Quiz.PassingThresholdPercent;
        await dbContext.SaveChangesAsync(ct);

        return new QuizAttemptResponseDto(
            attempt.Id,
            attempt.QuizId,
            attempt.StudentId,
            attempt.StartedAtUtc,
            attempt.SubmittedAtUtc,
            attempt.ScorePercent,
            attempt.IsPassed);
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
        var quiz = await dbContext.Quizzes.AsNoTracking()
            .Where(quiz => quiz.Id == quizId)
            .Select(quiz => new QuizResponseDto(
                quiz.Id,
                quiz.Title,
                quiz.MaxAttempts,
                quiz.TimeLimitSeconds,
                quiz.PassingThresholdPercent,
                quiz.SectionId))
            .FirstOrDefaultAsync(ct);

        uint courseId = await dbContext.Quizzes.AsNoTracking()
                            .Where(q => q.Id == quizId)
                            .Select(q => (uint?)q.Section.CourseId)
                            .FirstOrDefaultAsync(ct)
                        ?? throw ServiceException.NotFound(Resources.ErrorQuizNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(dbContext, courseId, userId, ct);

        return quiz ?? throw ServiceException.NotFound(Resources.ErrorQuizNotFound);
    }

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