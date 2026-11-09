namespace Backend.Models.DTOs.Response;

public record SectionResponseDto(
    uint Id,
    string Title,
    ushort Order,
    uint CourseId);