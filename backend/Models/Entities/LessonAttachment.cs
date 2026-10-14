using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities;

public class LessonAttachment {
    public int Id { get; set; }

    [Required] [MaxLength(256)]
    public string FileName { get; set; } = string.Empty;

    [Required] [Url] [MaxLength(2048)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContentType { get; set; }

    [Range(0, long.MaxValue)]
    public long? SizeBytes { get; set; }

    public int LessonId { get; set; }
    public required Lesson Lesson { get; set; }
}