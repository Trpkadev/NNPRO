using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Response;

public class StudentAnswerDto {
    [Required] [MaxLength(1024)]
    // JSON seznam ID vybraných variant, protože otázka může mít více odpovědí.
    public string SelectedOptionIdsJson { get; set; } = string.Empty;

    // Odpověď získá body pouze při úplné správnosti.
    public bool IsFullyCorrect { get; set; }

    [Range(0, ushort.MaxValue)]
    public ushort AwardedPoints { get; set; }
}