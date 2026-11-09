namespace Backend.Models.DTOs.Response;

public record LessonResponseDto(
    uint Id,
    string Title,
    string Content,
    string? VideoUrl,
    ushort Order,
    uint SectionId);

public record AttachmentResponseDto(
    uint Id,
    string FileName,
    string Url,
    string? ContentType,
    long? SizeBytes,
    uint LessonId);