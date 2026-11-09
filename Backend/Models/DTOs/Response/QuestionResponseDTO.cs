using Backend.Models.Entities;

namespace Backend.Models.DTOs.Response;

public record QuestionResponseDto(
    uint Id,
    string Text,
    string? ImageUrl,
    QuestionType Type,
    ushort Points,
    uint QuizId);