using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class Lesson {
    public uint Id { get; set; }

    [Required] [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    // Formátovaný text lekce včetně odkazů a dalšího vzdělávacího obsahu.
    [Required] [MaxLength(16_384)]
    public string Content { get; set; } = string.Empty;

    [Url] [MaxLength(512)]
    public string? VideoUrl { get; set; }

    [Range(0, ushort.MaxValue)]
    public ushort Order { get; set; }

    public uint SectionId { get; set; }
    public required Section Section { get; set; }
}