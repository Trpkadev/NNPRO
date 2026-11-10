using System.ComponentModel.DataAnnotations;
using Backend.Models.Entities;

namespace Backend.Models.DTOs.Request;

public sealed record UpdateUserRoleRequestDto(
    [Required] [EnumDataType(typeof(UserRole))] UserRole Role);