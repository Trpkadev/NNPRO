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

public class QuestionService(AppDbContext dbContext, IOptions<DatabaseLimitsOptions> dbLimitsOptions) : IQuestionService {
    public async Task<QuestionResponseDto> GetByIdAsync(uint questionId, uint userId, CancellationToken ct)
    {
        var result = await dbContext.Questions.AsNoTracking()
                         .Where(question => question.Id == questionId)
                         .Select(question => new {
                             Dto = new QuestionResponseDto(
                                 question.Id,
                                 question.Text,
                                 question.ImageUrl,
                                 question.Type,
                                 question.Points,
                                 question.QuizId),
                             question.Quiz.Section.CourseId
                         })
                         .FirstOrDefaultAsync(ct)
                     ?? throw ServiceException.NotFound(Resources.ErrorQuestionNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(dbContext, result.CourseId, userId, ct);

        return result.Dto;
    }

    public async Task<(AnswerOptionResponseDto[] Options, Metadata metadata)> GetOptionsAsync(uint questionId, uint userId, RequestQuery requestQuery, CancellationToken ct)
    {
        var questionInfo = await dbContext.Questions.AsNoTracking()
                               .Where(q => q.Id == questionId)
                               .Select(q => new {
                                   q.Quiz.Section.CourseId,
                                   q.Quiz.RandomizeAnswers
                               })
                               .FirstOrDefaultAsync(ct)
                           ?? throw ServiceException.NotFound(Resources.ErrorQuestionNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(dbContext, questionInfo.CourseId, userId, ct);

        var (options, metadata) = await dbContext.AnswerOptions.AsNoTracking()
            .Where(option => option.QuestionId == questionId)
            .OrderBy(option => option.Id)
            .Select(option => new AnswerOptionResponseDto(option.Id, option.Text, option.ImageUrl, option.QuestionId))
            .ToPagedResultAsync(requestQuery, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        if (questionInfo.RandomizeAnswers)
            Random.Shared.Shuffle(options);

        return (options, new Metadata { Pagination = metadata });
    }

    public async Task<(AnswerOptionEditorResponseDto[] Options, Metadata metadata)> GetOptionsForEditorAsync(uint requestingUserId, uint questionId, RequestQuery requestQuery, CancellationToken ct)
    {
        await EnsureCanManageQuestionAsync(requestingUserId, questionId, ct);
        var (options, metadata) = await dbContext.AnswerOptions.AsNoTracking()
            .Where(option => option.QuestionId == questionId)
            .OrderBy(option => option.Id)
            .Select(option => new AnswerOptionEditorResponseDto(option.Id, option.Text, option.ImageUrl, option.IsCorrect, option.QuestionId))
            .ToPagedResultAsync(requestQuery, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);
        return (options, new Metadata { Pagination = metadata });
    }

    public async Task<uint> CreateAnswerOptionAsync(uint requestingUserId, uint questionId, CreateAnswerOptionRequestDto request, CancellationToken ct)
    {
        await EnsureCanManageQuestionAsync(requestingUserId, questionId, ct);
        AnswerOption option = new() {
            Text = request.Text,
            ImageUrl = request.ImageUrl,
            IsCorrect = request.IsCorrect,
            QuestionId = questionId
        };
        dbContext.AnswerOptions.Add(option);
        await dbContext.SaveChangesAsync(ct);
        return option.Id;
    }

    public async Task UpdateAnswerOptionAsync(uint requestingUserId, uint questionId, uint optionId, UpdateAnswerOptionRequestDto request, CancellationToken ct)
    {
        await EnsureCanManageQuestionAsync(requestingUserId, questionId, ct);
        var option = await dbContext.AnswerOptions
            .FirstOrDefaultAsync(candidate => candidate.Id == optionId && candidate.QuestionId == questionId, ct);
        if (option is null)
            throw ServiceException.NotFound(Resources.ErrorAnswerOptionNotFound);

        option.Text = request.Text;
        option.ImageUrl = request.ImageUrl;
        option.IsCorrect = request.IsCorrect;
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAnswerOptionAsync(uint requestingUserId, uint questionId, uint optionId, CancellationToken ct)
    {
        await EnsureCanManageQuestionAsync(requestingUserId, questionId, ct);
        int deleted = await dbContext.AnswerOptions
            .Where(option => option.Id == optionId && option.QuestionId == questionId)
            .ExecuteDeleteAsync(ct);
        if (deleted == 0)
            throw ServiceException.NotFound(Resources.ErrorAnswerOptionNotFound);
    }

    public async Task<uint> CreateAsync(uint requestingUserId, CreateQuestionRequestDto request, CancellationToken ct)
    {
        await EnsureCanManageQuizAsync(requestingUserId, request.QuizId, ct);
        if (!Enum.TryParse(request.Type, true, out QuestionType questionType) || !Enum.IsDefined(questionType))
            throw ServiceException.BadRequest(Resources.ErrorQuestionTypeInvalid);

        Question question = new() {
            Text = request.Text,
            ImageUrl = request.ImageUrl,
            Type = questionType,
            Points = request.Points,
            QuizId = request.QuizId
        };
        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync(ct);
        return question.Id;
    }

    public async Task UpdateAsync(uint requestingUserId, uint questionId, CreateQuestionRequestDto request, CancellationToken ct)
    {
        var question = await dbContext.Questions.FirstOrDefaultAsync(candidate => candidate.Id == questionId, ct);
        if (question is null)
            throw ServiceException.NotFound(Resources.ErrorQuestionNotFound);

        await EnsureCanManageQuizAsync(requestingUserId, question.QuizId, ct);
        if (question.QuizId != request.QuizId)
            await EnsureCanManageQuizAsync(requestingUserId, request.QuizId, ct);
        if (!Enum.TryParse(request.Type, true, out QuestionType questionType) || !Enum.IsDefined(questionType))
            throw ServiceException.BadRequest(Resources.ErrorQuestionTypeInvalid);

        question.Text = request.Text;
        question.ImageUrl = request.ImageUrl;
        question.Type = questionType;
        question.Points = request.Points;
        question.QuizId = request.QuizId;
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(uint requestingUserId, uint questionId, CancellationToken ct)
    {
        await EnsureCanManageQuestionAsync(requestingUserId, questionId, ct);
        if (await dbContext.StudentAnswers.AnyAsync(answer => answer.QuestionId == questionId, ct))
            throw ServiceException.Conflict(Resources.ErrorQuestionHasSubmittedAnswers);

        await dbContext.Questions.Where(question => question.Id == questionId).ExecuteDeleteAsync(ct);
    }

    private async Task EnsureCanManageQuestionAsync(uint requestingUserId, uint questionId, CancellationToken ct)
    {
        uint? courseId = await dbContext.Questions.AsNoTracking()
            .Where(question => question.Id == questionId)
            .Select(question => (uint?)question.Quiz.Section.CourseId)
            .FirstOrDefaultAsync(ct);
        if (!courseId.HasValue)
            throw ServiceException.NotFound(Resources.ErrorQuestionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, courseId.Value, ct);
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
}