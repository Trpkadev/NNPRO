namespace Backend.Models.DTOs.Response;

public record UserResponseDto(
    uint Id,
    string Username,
    string Email,
    string Role);

public sealed record UserCourseResponseDto(
    uint Id,
    string Title,
    string ShortDescription);

public sealed record UserLessonProgressResponseDto(
    uint LessonId,
    string LessonTitle,
    uint SectionId,
    uint CourseId,
    bool IsCompleted,
    DateTime? CompletedAtUtc);