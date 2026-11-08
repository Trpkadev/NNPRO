using System.ComponentModel.DataAnnotations;

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
    public ushort? MaxDescriptionLength { get; init; }
}