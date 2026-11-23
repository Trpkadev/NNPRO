using Backend.Models.Entities;

namespace Backend.Models.DTOs.Response;

public record CourseResponseDto(
    uint Id,
    string Title,
    string Description,
    bool HasAccess);

public record CourseThumbnailResponseDto(
    uint Id,
    string Title,
    string ShortDescription,
    bool HasAccess);

public sealed record CourseInstructorResponseDto(
    uint Id,
    string Username);