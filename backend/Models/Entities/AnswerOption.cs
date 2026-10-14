using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities;

public class AnswerOption {
    public int Id { get; set; }

    [Required] [MaxLength(1024)]
    public string Text { get; set; } = string.Empty;

    [Url] [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    public bool IsCorrect { get; set; }

    public int QuestionId { get; set; }
    public required Question Question { get; set; }
}