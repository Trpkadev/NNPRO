using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Request;

public sealed class CourseRequestDto : RequestQuery;

public sealed class CoursePostRequestDto {
    [Required] [MaxLength(128)]
    public required string Title { get; init; }

    [Required] [MaxLength(1024)]
    public required string Description { get; init; }

    [MaxLength(128)]
    public string? AccessKey { get; init; }
}

public sealed class CourseUpdateRequestDto {
    [MaxLength(128)]
    public string? Title { get; init; }

    [MaxLength(1024)]
    public string? Description { get; init; }

    [MaxLength(128)]
    public string? AccessKey { get; init; }
}

public sealed class CourseThumbnailQuery : RequestQuery {
    public ushort? MaxDescriptionLength { get; init; }
}