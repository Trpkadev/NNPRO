using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class Course {
    public uint Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? AccessKey { get; set; }

    public ICollection<User> Instructors { get; set; } = [];
    public ICollection<Section> Sections { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}