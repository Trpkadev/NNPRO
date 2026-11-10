using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class User {
    public uint Id { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(32)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(128)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; } = UserRole.Student;

    public ICollection<Course> TeachingCourses { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<StudentLessonProgress> LessonProgresses { get; set; } = [];
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = [];
}