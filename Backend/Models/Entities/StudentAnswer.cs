using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class StudentAnswer {
    public uint Id { get; set; }

    public uint QuizAttemptId { get; set; }
    public QuizAttempt QuizAttempt { get; set; } = null!;

    public uint QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public uint[]? SelectedOptionIds { get; set; }

    public bool IsFullyCorrect { get; set; }

    [Range(0, ushort.MaxValue)]
    public ushort AwardedPoints { get; set; }
}