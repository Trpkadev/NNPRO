using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class Section {
    public uint Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [Range(0, ushort.MaxValue)]
    public ushort Order { get; set; }

    public uint CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public ICollection<Lesson> Lessons { get; set; } = [];
    public ICollection<Quiz> Quizzes { get; set; } = [];
}