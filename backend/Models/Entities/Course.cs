using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities;

public class Course {
    public int Id { get; set; }

    [Required] [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(128)]
    // Používá se pouze u uzavřených kurzů
    // null == veřejný kurz
    public string? AccessKey { get; set; }

    // Uživatelé, kteří kurz spravují
    public List<User> Instructors { get; set; } = [];
}