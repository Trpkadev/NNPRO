using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class AnswerOption {
    public uint Id { get; set; }

    [Required]
    [MaxLength(1024)]
    public string Text { get; set; } = string.Empty;

    [Url]
    [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    public bool IsCorrect { get; set; }

    public uint QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}