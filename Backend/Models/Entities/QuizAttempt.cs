using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class QuizAttempt : IValidatableObject {
    public uint Id { get; set; }

    public uint QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public uint StudentId { get; set; }
    public User Student { get; set; } = null!;

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAtUtc { get; set; }

    [Range(0, 100)]
    public decimal ScorePercent { get; set; }

    public bool IsPassed { get; set; }

    public ICollection<StudentAnswer> StudentAnswers { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SubmittedAtUtc.HasValue && SubmittedAtUtc.Value < StartedAtUtc)
            yield return new ValidationResult(Resources.ValidationQuizAttemptSubmissionBeforeStart, [nameof(StartedAtUtc), nameof(SubmittedAtUtc)]);

        if (!SubmittedAtUtc.HasValue && (ScorePercent != 0 || IsPassed))
            yield return new ValidationResult(Resources.ValidationUnsubmittedQuizAttemptMustNotHaveResult, [nameof(SubmittedAtUtc), nameof(ScorePercent), nameof(IsPassed)]);
    }
}