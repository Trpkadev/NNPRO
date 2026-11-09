namespace Backend.Models.DTOs.Response;

public record EnrollmentResponseDto(
    UserResponseDto[] EnrolledStudents);