using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Request;

public record EnrollmentRequestDto(
    [Range(1, uint.MaxValue)] uint CourseId,
    [MaxLength(128)] string? AccessKey);

public record ForceEnrollmentRequestDto(
    [Range(1, uint.MaxValue)] uint StudentId,
    [Range(1, uint.MaxValue)] uint CourseId);