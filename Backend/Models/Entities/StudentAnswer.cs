using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Backend.Models.Entities;

public class StudentAnswer : IValidatableObject {
    public uint Id { get; set; }

    public uint QuizAttemptId { get; set; }
    public QuizAttempt QuizAttempt { get; set; } = null!;

    public uint QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    [Required]
    [MaxLength(1024)]
    public string SelectedOptionIdsJson { get; set; } = string.Empty;

    public bool IsFullyCorrect { get; set; }

    [Range(0, ushort.MaxValue)]
    public ushort AwardedPoints { get; set; }

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