using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class StudentLessonProgress : IValidatableObject {
    public uint Id { get; set; }
    public uint StudentId { get; set; }
    public required User Student { get; set; }

    public uint LessonId { get; set; }
    public required Lesson Lesson { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsCompleted != CompletedAt.HasValue)
        {
            yield return new ValidationResult(
                "Dokončená lekce musí mít čas dokončení a nedokončená lekce ho mít nesmí.",
                [nameof(IsCompleted), nameof(CompletedAt)]);
        }
    }
}