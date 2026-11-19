using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface IQuizService {
    Task<QuizResponseDto> GetByIdAsync(uint quizId, CancellationToken ct);

    Task<QuestionResponseDto[]> GetQuestionsAsync(uint quizId, CancellationToken ct);

    Task<uint> CreateAsync(uint requestingUserId, CreateQuizRequestDto request, CancellationToken ct);

    Task UpdateAsync(uint requestingUserId, uint quizId, CreateQuizRequestDto request, CancellationToken ct);

    Task DeleteAsync(uint requestingUserId, uint quizId, CancellationToken ct);

    Task<QuizResponseDto> GetForEditorAsync(uint requestingUserId, uint quizId, CancellationToken ct);

    Task<QuizAttemptResponseDto[]> GetAttemptsAsync(uint requestingUserId, uint quizId, CancellationToken ct);

    Task<MyQuizAttemptResponseDto[]> GetMyAttemptsAsync(uint studentId, uint quizId, CancellationToken ct);

    Task<QuizAttemptResponseDto> GetAttemptAsync(uint requestingUserId, uint attemptId, CancellationToken ct);

    Task<uint> StartAttemptAsync(uint studentId, StartAttemptRequestDto request, CancellationToken ct);

    Task<QuizAttemptResponseDto> EndAttemptAsync(uint studentId, EndAttemptRequestDto request, CancellationToken ct);
}