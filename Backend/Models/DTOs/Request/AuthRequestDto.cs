using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Request;

public sealed record LoginRequestDto(
    [MinLength(3)] [MaxLength(32)] string? Username,
    [EmailAddress] [MaxLength(128)] string? Email,
    [Required] string Password,
    [MaxLength(100)] string? DeviceName = null) : IValidatableObject {
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Email))
            yield return new ValidationResult(Resources.ValidationLoginIdentifierRequired, [nameof(Username), nameof(Email)]);
    }
}

public sealed record RegisterRequestDto(
    [Required] [MinLength(3)] [MaxLength(32)] string Username,
    [Required] [EmailAddress] [MaxLength(128)] string Email,
    [Required] [MinLength(8)] [MaxLength(256)] string Password,
    [MaxLength(100)] string? DeviceName = null);

public sealed record RefreshTokenRequestDto(
    [Required] [MaxLength(128)] string RefreshToken);