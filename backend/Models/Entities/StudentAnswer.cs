using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace backend.Models.Entities;

public class StudentAnswer : IValidatableObject {
    public int Id { get; set; }
    public int QuizAttemptId { get; set; }
    public required QuizAttempt QuizAttempt { get; set; }

    public int QuestionId { get; set; }
    public required Question Question { get; set; }

    [Required] [MaxLength(1024)]
    // JSON seznam ID vybraných variant, protože otázka může mít více odpovědí.
    public string SelectedOptionIdsJson { get; set; } = string.Empty;

    // Odpověď získá body pouze při úplné správnosti.
    public bool IsFullyCorrect { get; set; }

    [Range(0, int.MaxValue)]
    public int AwardedPoints { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsValidOptionArray(SelectedOptionIdsJson))
        {
            yield return new ValidationResult(
                "SelectedOptionIdsJson musí obsahovat platné JSON pole.",
                [nameof(SelectedOptionIdsJson)]);
        }
    }

    private static bool IsValidOptionArray(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}