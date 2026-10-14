using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities;

public class StudentLessonProgress : IValidatableObject {
    public int Id { get; set; }
    public int StudentId { get; set; }
    public required User Student { get; set; }

    public int LessonId { get; set; }
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