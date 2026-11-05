using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Response;

public class LessonAttachmentDto {
    [Required] [MaxLength(256)]
    public string FileName { get; set; } = string.Empty;

    [Required] [Url] [MaxLength(2048)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContentType { get; set; }

    [Range(0, ulong.MaxValue)]
    public ulong? SizeBytes { get; set; }
}