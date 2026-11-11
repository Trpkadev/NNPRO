using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class StudentLessonProgress : IValidatableObject {
    public uint Id { get; set; }

    public uint StudentId { get; set; }
    public User Student { get; set; } = null!;

    public uint LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsCompleted != CompletedAtUtc.HasValue)
            yield return new ValidationResult(
                Resources.ValidationStudentLessonProgressCompletionTimestamp,
                [nameof(IsCompleted), nameof(CompletedAtUtc)]);
    }
}