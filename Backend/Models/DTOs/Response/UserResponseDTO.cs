using System.ComponentModel.DataAnnotations;
using Backend.Models.Entities;

namespace Backend.Models.DTOs.Response;

public class UserResponseDto {
    [Required] [MinLength(3)] [MaxLength(32)]
    public string Username { get; set; } = string.Empty;

    [Required] [EmailAddress] [MaxLength(128)]
    public string Email { get; set; } = string.Empty;

    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; }
}