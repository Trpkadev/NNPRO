using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.Configuration;

public sealed record JwtOptions : IValidatableObject {
    [Required]
    public required string SecretKey { get; init; }

    [Required]
    public required string Issuer { get; init; }

    [Required]
    public required string Audience { get; init; }

    [Range(1, int.MaxValue)]
    public int AccessTokenExpirationMinutes { get; init; }

    [Range(1, int.MaxValue)]
    public int RefreshTokenExpirationDays { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(SecretKey) && Encoding.UTF8.GetByteCount(SecretKey) < 32)
            yield return new ValidationResult(Resources.ErrorJwtSigningKeyTooShort, [nameof(SecretKey)]);
    }
}