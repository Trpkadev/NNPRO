using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities;

public class Section {
    public int Id { get; set; }

    [Required] [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Order { get; set; } // Pořadí sekce v kurzu

    public int CourseId { get; set; }
    public required Course Course { get; set; }
}