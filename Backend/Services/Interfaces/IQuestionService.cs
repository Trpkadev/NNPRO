using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface IQuestionService {
    Task<QuestionResponseDto> GetByIdAsync(uint questionId, uint userId, CancellationToken ct);

    Task<(AnswerOptionResponseDto[] Options, Metadata metadata)> GetOptionsAsync(uint questionId, uint userId, RequestQuery requestQuery, CancellationToken ct);

    Task<(AnswerOptionEditorResponseDto[] Options, Metadata metadata)> GetOptionsForEditorAsync(uint requestingUserId, uint questionId, RequestQuery requestQuery, CancellationToken ct);

    Task<uint> CreateAnswerOptionAsync(uint requestingUserId, uint questionId, CreateAnswerOptionRequestDto request, CancellationToken ct);

    Task UpdateAnswerOptionAsync(uint requestingUserId, uint questionId, uint optionId, UpdateAnswerOptionRequestDto request, CancellationToken ct);

    Task DeleteAnswerOptionAsync(uint requestingUserId, uint questionId, uint optionId, CancellationToken ct);

    Task<uint> CreateAsync(uint requestingUserId, CreateQuestionRequestDto request, CancellationToken ct);

    Task UpdateAsync(uint requestingUserId, uint questionId, CreateQuestionRequestDto request, CancellationToken ct);

    Task DeleteAsync(uint requestingUserId, uint questionId, CancellationToken ct);
}