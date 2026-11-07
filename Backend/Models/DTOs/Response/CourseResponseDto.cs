using Backend.Models.Entities;

namespace Backend.Models.DTOs.Response;

public record CourseResponseDto(uint Id, string Title, string Description, string? AccessKey) {
    public static implicit operator CourseResponseDto(Course c) => new(c.Id, c.Title, c.Description, c.AccessKey);
}

public record CourseThumbnailDto(string Title, string ShortDescription) {
    public static implicit operator CourseThumbnailDto(Course c) => new(c.Title, c.Description);
}