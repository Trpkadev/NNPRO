using System.ComponentModel.DataAnnotations;
using Backend.Models.Entities;

namespace Backend.Models.DTOs.Response;

public class QuestionDto {
    [Required] [MaxLength(2048)]
    public string Text { get; set; } = string.Empty; // Může obsahovat formátovaný text

    [Url] [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    [EnumDataType(typeof(QuestionType))]
    public QuestionType Type { get; set; }

    [Range(1, ushort.MaxValue)]
    public ushort Points { get; set; }
}