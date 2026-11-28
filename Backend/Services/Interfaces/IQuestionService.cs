using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface IQuestionService {
    Task<QuestionResponseDto> GetByIdAsync(uint questionId, CancellationToken ct);

    Task<(AnswerOptionResponseDto[] Options, Metadata metadata)> GetOptionsAsync(uint questionId, RequestQuery requestQuery, CancellationToken ct);

    Task<(AnswerOptionEditorResponseDto[] Options, Metadata metadata)> GetOptionsForEditorAsync(uint questionId, RequestQuery requestQuery, CancellationToken ct);

    Task<uint> CreateAnswerOptionAsync(uint questionId, CreateAnswerOptionRequestDto request, CancellationToken ct);

    Task UpdateAnswerOptionAsync(uint questionId, uint optionId, UpdateAnswerOptionRequestDto request, CancellationToken ct);

    Task DeleteAnswerOptionAsync(uint questionId, uint optionId, CancellationToken ct);

    Task<uint> CreateAsync(CreateQuestionRequestDto request, CancellationToken ct);

    Task UpdateAsync(uint questionId, CreateQuestionRequestDto request, CancellationToken ct);

    Task DeleteAsync(uint questionId, CancellationToken ct);
}