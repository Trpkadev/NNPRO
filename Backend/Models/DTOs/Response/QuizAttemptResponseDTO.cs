namespace Backend.Models.DTOs.Response;

public sealed record QuizAttemptResponseDto(
    uint Id,
    uint QuizId,
    uint StudentId,
    DateTime StartedAtUtc,
    DateTime? SubmittedAtUtc,
    decimal ScorePercent,
    bool IsPassed);

public sealed record QuizAttemptDetailResponseDto(
    uint Id,
    uint QuizId,
    uint StudentId,
    DateTime StartedAtUtc,
    DateTime? SubmittedAtUtc,
    decimal ScorePercent,
    bool IsPassed,
    Dictionary<uint, uint[]?> AnswerIdsByQuestionId);

public sealed record MyQuizAttemptResponseDto(
    uint Id,
    DateTime StartedAtUtc,
    DateTime? SubmittedAtUtc,
    decimal ScorePercent,
    bool IsPassed);