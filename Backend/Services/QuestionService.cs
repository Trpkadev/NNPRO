using Backend.Exceptions;
using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class QuestionService(ServiceContext<QuestionService> ctx) : IQuestionService {
    public async Task<QuestionResponseDto> GetByIdAsync(uint questionId, CancellationToken ct)
    {
        var result = await ctx.Db.Questions.AsNoTracking()
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

        await CourseAuthorization.EnsureCanAccessCourseAsync(ctx.Db, result.CourseId, ctx.AuthUserId, ct);

        return result.Dto;
    }

    public async Task<(AnswerOptionResponseDto[] Options, Metadata metadata)> GetOptionsAsync(uint questionId, RequestQuery requestQuery, CancellationToken ct)
    {
        var questionInfo = await ctx.Db.Questions.AsNoTracking()
                               .Where(q => q.Id == questionId)
                               .Select(q => new {
                                   q.Quiz.Section.CourseId,
                                   q.Quiz.RandomizeAnswers
                               })
                               .FirstOrDefaultAsync(ct)
                           ?? throw ServiceException.NotFound(Resources.ErrorQuestionNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(ctx.Db, questionInfo.CourseId, ctx.AuthUserId, ct);

        var (options, metadata) = await ctx.Db.AnswerOptions.AsNoTracking()
            .Where(option => option.QuestionId == questionId)
            .OrderBy(option => option.Id)
            .Select(option => new AnswerOptionResponseDto(option.Id, option.Text, option.ImageUrl, option.QuestionId))
            .ToPagedResultAsync(requestQuery, ctx.MaxUnpagedItems, ct);

        if (questionInfo.RandomizeAnswers)
            Random.Shared.Shuffle(options);

        return (options, new Metadata(metadata, requestQuery));
    }

    public async Task<(AnswerOptionEditorResponseDto[] Options, Metadata metadata)> GetOptionsForEditorAsync(uint questionId, RequestQuery requestQuery, CancellationToken ct)
    {
        await EnsureCanManageQuestionAsync(questionId, ct);
        var (options, metadata) = await ctx.Db.AnswerOptions.AsNoTracking()
            .Where(option => option.QuestionId == questionId)
            .OrderBy(option => option.Id)
            .Select(option => new AnswerOptionEditorResponseDto(option.Id, option.Text, option.ImageUrl, option.IsCorrect, option.QuestionId))
            .ToPagedResultAsync(requestQuery, ctx.MaxUnpagedItems, ct);
        return (options, new Metadata(metadata, requestQuery));
    }

    public async Task<uint> CreateAnswerOptionAsync(uint questionId, CreateAnswerOptionRequestDto request, CancellationToken ct)
    {
        await EnsureCanManageQuestionAsync(questionId, ct);
        AnswerOption option = new() {
            Text = request.Text,
            ImageUrl = request.ImageUrl,
            IsCorrect = request.IsCorrect,
            QuestionId = questionId
        };
        ctx.Db.AnswerOptions.Add(option);
        await ctx.Db.SaveChangesAsync(ct);
        return option.Id;
    }

    public async Task UpdateAnswerOptionAsync(uint questionId, uint optionId, UpdateAnswerOptionRequestDto request, CancellationToken ct)
    {
        await EnsureCanManageQuestionAsync(questionId, ct);
        var option = await ctx.Db.AnswerOptions
            .FirstOrDefaultAsync(candidate => candidate.Id == optionId && candidate.QuestionId == questionId, ct);
        if (option is null)
            throw ServiceException.NotFound(Resources.ErrorAnswerOptionNotFound);

        option.Text = request.Text;
        option.ImageUrl = request.ImageUrl;
        option.IsCorrect = request.IsCorrect;
        await ctx.Db.SaveChangesAsync(ct);
    }

    public async Task DeleteAnswerOptionAsync(uint questionId, uint optionId, CancellationToken ct)
    {
        await EnsureCanManageQuestionAsync(questionId, ct);
        int deleted = await ctx.Db.AnswerOptions
            .Where(option => option.Id == optionId && option.QuestionId == questionId)
            .ExecuteDeleteAsync(ct);
        if (deleted == 0)
            throw ServiceException.NotFound(Resources.ErrorAnswerOptionNotFound);
    }

    public async Task<uint> CreateAsync(CreateQuestionRequestDto request, CancellationToken ct)
    {
        await EnsureCanManageQuizAsync(request.QuizId, ct);
        if (!Enum.TryParse(request.Type, true, out QuestionType questionType) || !Enum.IsDefined(questionType))
            throw ServiceException.BadRequest(Resources.ErrorQuestionTypeInvalid);

        Question question = new() {
            Text = request.Text,
            ImageUrl = request.ImageUrl,
            Type = questionType,
            Points = request.Points,
            QuizId = request.QuizId
        };
        ctx.Db.Questions.Add(question);
        await ctx.Db.SaveChangesAsync(ct);
        return question.Id;
    }

    public async Task UpdateAsync(uint questionId, CreateQuestionRequestDto request, CancellationToken ct)
    {
        var question = await ctx.Db.Questions.FirstOrDefaultAsync(candidate => candidate.Id == questionId, ct);
        if (question is null)
            throw ServiceException.NotFound(Resources.ErrorQuestionNotFound);

        await EnsureCanManageQuizAsync(question.QuizId, ct);
        if (question.QuizId != request.QuizId)
            await EnsureCanManageQuizAsync(request.QuizId, ct);
        if (!Enum.TryParse(request.Type, true, out QuestionType questionType) || !Enum.IsDefined(questionType))
            throw ServiceException.BadRequest(Resources.ErrorQuestionTypeInvalid);

        question.Text = request.Text;
        question.ImageUrl = request.ImageUrl;
        question.Type = questionType;
        question.Points = request.Points;
        question.QuizId = request.QuizId;
        await ctx.Db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(uint questionId, CancellationToken ct)
    {
        await EnsureCanManageQuestionAsync(questionId, ct);
        if (await ctx.Db.AttempterAnswers.AnyAsync(answer => answer.QuestionId == questionId, ct))
            throw ServiceException.Conflict(Resources.ErrorQuestionHasSubmittedAnswers);

        await ctx.Db.Questions.Where(question => question.Id == questionId).ExecuteDeleteAsync(ct);
    }

    private async Task EnsureCanManageQuestionAsync(uint questionId, CancellationToken ct) => await EnsureCanManageQuestionAsync(ctx.AuthUserId, questionId, ct);

    private async Task EnsureCanManageQuestionAsync(uint userId, uint questionId, CancellationToken ct)
    {
        uint courseId = await ctx.Db.Questions.AsNoTracking()
                            .Where(question => question.Id == questionId)
                            .Select(question => (uint?)question.Quiz.Section.CourseId)
                            .FirstOrDefaultAsync(ct)
                        ?? throw ServiceException.NotFound(Resources.ErrorQuestionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, courseId, userId, ct);
    }

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
}