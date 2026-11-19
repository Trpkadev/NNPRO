using Backend.Config;
using Backend.Data;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Microsoft.Extensions.Options;

namespace Backend.Services.Interfaces;

public interface IQuizService {
    Task<QuizResponseDto> GetByIdAsync(uint quizId, uint userId, CancellationToken ct);

    Task<(QuestionResponseDto[] Questions, Metadata Metadata)> GetQuestionsAsync(uint quizId, uint userId, RequestQuery requestQuery, CancellationToken ct);

    Task<uint> CreateAsync(uint requestingUserId, CreateQuizRequestDto request, CancellationToken ct);

    Task UpdateAsync(uint requestingUserId, uint quizId, CreateQuizRequestDto request, CancellationToken ct);

    Task DeleteAsync(uint requestingUserId, uint quizId, CancellationToken ct);

    Task<QuizResponseDto> GetForEditorAsync(uint requestingUserId, uint quizId, CancellationToken ct);

    Task<(QuizAttemptResponseDto[] Attempts, Metadata Metadata)> GetAttemptsAsync(uint requestingUserId, uint quizId, RequestQuery request, CancellationToken ct);

    Task<(MyQuizAttemptResponseDto[] Attempts, Metadata Metadata)> GetMyAttemptsAsync(uint studentId, uint quizId, RequestQuery request, CancellationToken ct);

    Task<QuizAttemptResponseDto> GetAttemptByAttemptIdAsync(uint requestingUserId, uint attemptId, CancellationToken ct);

    Task<uint> StartAttemptAsync(uint studentId, StartAttemptRequestDto request, CancellationToken ct);

    Task<QuizAttemptResponseDto> EndAttemptAsync(uint studentId, EndAttemptRequestDto request, CancellationToken ct);
}

public abstract class BaseService(AppDbContext dbContext, IOptions<DatabaseLimitsOptions> dbLimitsOptions, ILogger<BaseService> logger);