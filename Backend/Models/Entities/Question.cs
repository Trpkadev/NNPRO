using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class Question {
    public uint Id { get; set; }

    [Required]
    [MaxLength(2048)]
    public string Text { get; set; } = string.Empty;

    [Url]
    [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    [EnumDataType(typeof(QuestionType))]
    public QuestionType Type { get; set; }

    [Range(1, ushort.MaxValue)]
    public ushort Points { get; set; }

    public uint QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    // Navigace
    public ICollection<AnswerOption> AnswerOptions { get; set; } = [];
}