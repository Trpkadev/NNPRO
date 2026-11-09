using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Request;

public record CreateLessonRequestDto(
    [Required] [MaxLength(128)] string Title,
    [MaxLength(16_384)] string Content,
    [Url] [MaxLength(512)] string? VideoUrl,
    [Range(0, ushort.MaxValue)] ushort Order,
    [Range(1, uint.MaxValue)] uint SectionId);

public record AttachmentRequestDto(
    [Range(1, uint.MaxValue)] uint LessonId,
    [Required] [MaxLength(256)] string FileName,
    [Required] [Url] [MaxLength(2048)] string Url,
    [MaxLength(100)] string? ContentType,
    [Range(0, long.MaxValue)] long? SizeBytes);

public record UpdateProgressDto(
    [Required] bool IsCompleted);