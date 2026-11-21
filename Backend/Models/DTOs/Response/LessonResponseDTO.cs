namespace Backend.Models.DTOs.Response;

public record LessonResponseDto(
    uint Id,
    string Title,
    string Content,
    string? VideoUrl,
    ushort Order,
    uint SectionId);

public record LessonResponseWrapperDto(
    LessonResponseDto[] LessonResponseDto,
    uint TotalLessons,
    uint CompletedLessons);

public record AttachmentResponseDto(
    uint Id,
    string FileName,
    string Url,
    string? ContentType,
    long? SizeBytes,
    uint LessonId);