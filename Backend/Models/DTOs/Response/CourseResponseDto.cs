using Backend.Models.Entities;

namespace Backend.Models.DTOs.Response;

public record CourseResponseDto(
    uint Id,
    string Title,
    string Description,
    string? AccessKey) {
    public static implicit operator CourseResponseDto(Course c) => new(c.Id, c.Title, c.Description, c.AccessKey);
}

public record CourseThumbnailResponseDto(
    uint Id,
    string Title,
    string ShortDescription) {
    public static implicit operator CourseThumbnailResponseDto(Course c) => new(c.Id, c.Title, c.Description);
}