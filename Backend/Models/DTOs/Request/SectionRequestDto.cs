using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Request;

public record CreateSectionRequestDto(
    [Required] [MaxLength(128)] string Title,
    [Range(0, ushort.MaxValue)] ushort Order,
    [Range(1, uint.MaxValue)] uint CourseId);