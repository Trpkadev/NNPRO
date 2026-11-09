namespace Backend.Models.DTOs.Response;

public record QuizResponseDto(
    uint Id,
    string Title,
    ushort? MaxAttempts,
    uint? TimeLimitSeconds,
    decimal PassingThresholdPercent,
    uint SectionId);