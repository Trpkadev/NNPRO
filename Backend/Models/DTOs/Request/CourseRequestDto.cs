using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Request;

public sealed class CourseRequestDto : RequestQuery;

public sealed class CoursePostRequestDto {
    [Required] [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [Required] [MaxLength(1024)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? AccessKey { get; set; }
}

public sealed class CourseUpdateRequestDto {
    [Required] [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [Required] [MaxLength(1024)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? AccessKey { get; set; }
}

public sealed class CourseThumbnailQuery : RequestQuery {
    public ushort? MaxDescriptionLength { get; init; }
}