using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Response;

public class AnswerOptionDto {
    [Required] [MaxLength(1024)]
    public string Text { get; set; } = string.Empty;

    [Url] [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    public bool IsCorrect { get; set; }
}