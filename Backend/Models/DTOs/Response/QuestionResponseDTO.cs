using Backend.Models.Entities;

namespace Backend.Models.DTOs.Response;

public record QuestionResponseDto(
    uint Id,
    string Text,
    string? ImageUrl,
    QuestionType Type,
    ushort Points,
    uint QuizId);

public sealed record AnswerOptionResponseDto(
    uint Id,
    string Text,
    string? ImageUrl,
    uint QuestionId);

public sealed record AnswerOptionEditorResponseDto(
    uint Id,
    string Text,
    string? ImageUrl,
    bool IsCorrect,
    uint QuestionId);