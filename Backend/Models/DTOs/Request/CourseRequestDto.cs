using System.ComponentModel.DataAnnotations;
using Backend.Models.DTOs.Response;

namespace Backend.Models.DTOs.Request;

public sealed record CourseCreateRequestDto {
    [Required]
    [MaxLength(128)]
    public required string Title { get; init; }

    [Required]
    [MaxLength(1024)]
    public required string Description { get; init; }

    [MaxLength(128)]
    public string? AccessKey { get; init; }
}

public sealed record CourseUpdateRequestDto {
    [Required]
    [MaxLength(128)]
    public required string Title { get; init; }

    [Required]
    [MaxLength(1024)]
    public required string Description { get; init; }

    [MaxLength(128)]
    public string? AccessKey { get; init; }
}

public sealed record CourseThumbnailRequestDto : RequestQuery {
    private static readonly HashSet<string> AllowedSortProperties = typeof(CourseThumbnailResponseDto).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
    public ushort? MaxDescriptionLength { get; init; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in base.Validate(validationContext))
            yield return result;

        if (string.IsNullOrWhiteSpace(SortBy))
            yield break;

        if (!AllowedSortProperties.Contains(SortBy))
            yield return new ValidationResult(string.Format(Resources.ErrorDescriptionInvalidSortBy, SortBy, "Course"), [nameof(SortBy)]);
    }
}