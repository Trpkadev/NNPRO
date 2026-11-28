using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface IQuizService {
    Task<QuizResponseDto> GetByIdAsync(uint quizId, CancellationToken ct);

    Task<(QuestionResponseDto[] Questions, Metadata Metadata)> GetQuestionsAsync(uint quizId, RequestQuery requestQuery, CancellationToken ct);

    Task<uint> CreateAsync(CreateQuizRequestDto request, CancellationToken ct);

    Task UpdateAsync(uint quizId, CreateQuizRequestDto request, CancellationToken ct);

    Task DeleteAsync(uint quizId, CancellationToken ct);

    Task<QuizResponseDto> GetForEditorAsync(uint quizId, CancellationToken ct);

    Task<(QuizAttemptResponseDto[] Attempts, Metadata Metadata)> GetAttemptsAsync(uint quizId, RequestQuery request, CancellationToken ct);

    Task<(MyQuizAttemptResponseDto[] Attempts, Metadata Metadata)> GetMyAttemptsAsync(uint quizId, RequestQuery request, CancellationToken ct);

    Task<QuizAttemptDetailResponseDto> GetAttemptByAttemptIdAsync(uint attemptId, CancellationToken ct);

    Task<uint> StartAttemptAsync(StartAttemptRequestDto request, CancellationToken ct);

    Task<QuizAttemptResponseDto> EndAttemptAsync(EndAttemptRequestDto request, CancellationToken ct);
}