using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Response;

public class CourseDto {
    [Required] [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? AccessKey { get; set; }
}

public class CourseThumbnailDto {
    [Required]
    [MaxLength(128)]
    public required string Title { get; init; }

    [MaxLength(1024)]
    public required string ShortDescription { get; init; }
}