using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities;

public class Lesson {
    public int Id { get; set; }

    [Required] [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    // Formátovaný text lekce včetně odkazů a dalšího vzdělávacího obsahu.
    [Required] [MaxLength(16_384)]
    public string Content { get; set; } = string.Empty;

    [Url] [MaxLength(512)]
    public string? VideoUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int Order { get; set; }

    public int SectionId { get; set; }
    public required Section Section { get; set; }
}