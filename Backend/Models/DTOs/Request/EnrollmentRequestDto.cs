using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Request;

public record EnrollmentRequestDto(
    [Required] uint CourseId,
    [MaxLength(128)] string? AccessKey);

public record ForceEnrollmentRequestDto(
    [Required] uint StudentId,
    [Required] uint CourseId);