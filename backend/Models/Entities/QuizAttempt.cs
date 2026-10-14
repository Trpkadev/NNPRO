using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities;

public class QuizAttempt : IValidatableObject {
    public int Id { get; set; }
    public int QuizId { get; set; }
    public required Quiz Quiz { get; set; }

    public int StudentId { get; set; }
    public required User Student { get; set; }

    // Začátek pokusu v UTC.
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    // Null znamená, že student pokus ještě neodevzdal.
    public DateTime? SubmittedAt { get; set; }

    [Range(0, 100)]
    // Výsledek pokusu v procentech (0-100).
    public double ScorePercent { get; set; }

    // Určuje, zda výsledek dosáhl prahu nastaveného u kvízu.
    public bool IsPassed { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SubmittedAt.HasValue && SubmittedAt.Value < StartedAt)
        {
            yield return new ValidationResult(
                "Čas odevzdání nesmí předcházet času zahájení pokusu.",
                [nameof(StartedAt), nameof(SubmittedAt)]);
        }

        if (!SubmittedAt.HasValue && (ScorePercent != 0 || IsPassed))
        {
            yield return new ValidationResult(
                "Neodevzdaný pokus nesmí mít výsledek ani být označen jako splněný.",
                [nameof(SubmittedAt), nameof(ScorePercent), nameof(IsPassed)]);
        }
    }
}