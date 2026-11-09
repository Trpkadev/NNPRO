namespace Backend.Models.DTOs.Response;

public record QuizAttemptResponseDto(
    uint Id,
    uint QuizId,
    uint StudentId,
    DateTime StartedAtUtc,
    DateTime? SubmittedAtUtc,
    decimal ScorePercent,
    bool IsPassed);

public sealed record MyQuizAttemptResponseDto(
    uint Id,
    DateTime StartedAtUtc,
    DateTime? SubmittedAtUtc,
    decimal ScorePercent,
    bool IsPassed);