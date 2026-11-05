using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Response;

public class QuizAttemptDto {
    // Začátek pokusu v UTC.
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    // Null znamená, že student pokus ještě neodevzdal.
    public DateTime? SubmittedAt { get; set; }

    [Range(0, 100)]
    // Výsledek pokusu v procentech (0-100).
    public double ScorePercent { get; set; }

    // Určuje, zda výsledek dosáhl prahu nastaveného u kvízu.
    public bool IsPassed { get; set; }
}