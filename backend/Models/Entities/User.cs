using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities;

public class User {
    public int Id { get; set; }

    [Required] [MinLength(3)] [MaxLength(32)]
    public string Username { get; set; } = string.Empty;

    [Required] [EmailAddress] [MaxLength(128)]
    public string Email { get; set; } = string.Empty;

    [Required] [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; }
}