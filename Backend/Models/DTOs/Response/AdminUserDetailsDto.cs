namespace Backend.Models.DTOs.Response;

public sealed record AdminUserDetailsDto(
    uint Id,
    string Username,
    string Email,
    string Role,
    int EnrollmentCount,
    int LessonProgressCount,
    int QuizAttemptCount,
    int ActiveSessionCount);