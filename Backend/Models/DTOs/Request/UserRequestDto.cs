using System.ComponentModel.DataAnnotations;
using Backend.Models.Entities;

namespace Backend.Models.DTOs.Request;

public record CreateUserRequestDto(
    [Required] [MinLength(3)] [MaxLength(32)] string Username,
    [Required] [EmailAddress] [MaxLength(128)] string Email,
    [Required] [MinLength(8)] [MaxLength(256)] string Password,
    [Required] [EnumDataType(typeof(UserRole))] string Role);

public record UpdateUserRequestDto(
    [Required] [MinLength(3)] [MaxLength(32)] string Username,
    [Required] [EmailAddress] [MaxLength(128)] string Email,
    [Required] [MinLength(8)] [MaxLength(256)] string Password);