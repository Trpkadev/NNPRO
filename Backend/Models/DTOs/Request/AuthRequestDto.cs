using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Request;

public sealed record LoginRequestDto(
    [MinLength(3)] [MaxLength(32)] string? Username,
    [EmailAddress] [MaxLength(128)] string? Email,
    [Required] string Password) : IValidatableObject {
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Email))
            yield return new ValidationResult("Must provide either Username or Email for login.", [nameof(Username), nameof(Email)]);
    }
}

public sealed record RegisterRequestDto(
    [Required] [MinLength(3)] [MaxLength(32)] string Username,
    [Required] [EmailAddress] [MaxLength(128)] string Email,
    [Required] [MinLength(8)] [MaxLength(256)] string Password);

public sealed record RefreshTokenRequestDto(
    [Required] string AccessToken,
    [Required] string RefreshToken);

public sealed record LogoutRequestDto(
    [Required] string RefreshToken);