using Backend.Models.Entities;

namespace Backend.Models.DTOs.Response;

public record CourseResponseDto(
    uint Id,
    string Title,
    string Description);

public record CourseThumbnailResponseDto(
    uint Id,
    string Title,
    string ShortDescription) {
    public static implicit operator CourseThumbnailResponseDto(Course c) => new(c.Id, c.Title, c.Description);
}

public sealed record CourseInstructorResponseDto(
    uint Id,
    string Username);