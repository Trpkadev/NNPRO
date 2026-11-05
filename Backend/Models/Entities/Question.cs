using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class Question {
    public uint Id { get; set; }

    [Required] [MaxLength(2048)]
    public string Text { get; set; } = string.Empty; // Může obsahovat formátovaný text

    [Url] [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    [EnumDataType(typeof(QuestionType))]
    public QuestionType Type { get; set; }

    [Range(1, ushort.MaxValue)]
    public ushort Points { get; set; }

    public uint QuizId { get; set; }
    public required Quiz Quiz { get; set; }
}