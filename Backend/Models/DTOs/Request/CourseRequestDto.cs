using System.ComponentModel.DataAnnotations;
using Backend.Models.DTOs.Response;

namespace Backend.Models.DTOs.Request;

public sealed record CourseCreateRequestDto(
    [Required] [MaxLength(128)] string Title,
    [Required] [MaxLength(1024)] string Description,
    [MaxLength(128)] string? AccessKey);

public sealed record CourseUpdateRequestDto(
    [Required] [MaxLength(128)] string Title,
    [Required] [MaxLength(1024)] string Description,
    [MaxLength(128)] string? AccessKey);

public sealed record CourseThumbnailRequestDto(
    [Range(10, 500)] ushort? MaxDescriptionLength) : RequestQuery {
    private static readonly HashSet<string> AllowedSortProperties = typeof(CourseThumbnailResponseDto).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in base.Validate(validationContext))
            yield return result;

        if (string.IsNullOrWhiteSpace(SortBy))
            yield break;

        if (!AllowedSortProperties.Contains(SortBy))
            yield return new ValidationResult($"Invalid sort property: {SortBy}", [nameof(SortBy)]);
    }
}