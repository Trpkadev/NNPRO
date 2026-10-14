using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities;

public class Question {
    public int Id { get; set; }

    [Required] [MaxLength(2048)]
    public string Text { get; set; } = string.Empty; // Může obsahovat formátovaný text

    [Url] [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    [EnumDataType(typeof(QuestionType))]
    public QuestionType Type { get; set; }

    [Range(1, int.MaxValue)]
    public int Points { get; set; }

    public int QuizId { get; set; }
    public required Quiz Quiz { get; set; }
}