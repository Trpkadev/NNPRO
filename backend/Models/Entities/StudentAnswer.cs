using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities;

public class StudentAnswer {
    public int Id { get; set; }
    public int QuizAttemptId { get; set; }
    public required QuizAttempt QuizAttempt { get; set; }

    public int QuestionId { get; set; }
    public required Question Question { get; set; }

    public ICollection<AnswerOption> SelectedOptions { get; set; } = [];

    // Odpověď získá body pouze při úplné správnosti.
    public bool IsFullyCorrect { get; set; }

    [Range(0, int.MaxValue)]
    public int AwardedPoints { get; set; }
}