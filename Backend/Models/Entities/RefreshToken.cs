using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Entities;

public class RefreshToken {
    public uint Id { get; set; }

    [Required]
    [MaxLength(64)]
    public required string TokenHash { get; set; }

    [Required]
    [MaxLength(32)]
    public required string JwtId { get; set; }

    public Guid TokenFamilyId { get; set; }

    public uint UserId { get; set; }
    public User User { get; set; } = null!;

    public required DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(45)]
    public string? CreatedByIp { get; set; }

    [MaxLength(512)]
    public string? UserAgent { get; set; }

    [MaxLength(100)]
    public string? DeviceName { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    [MaxLength(45)]
    public string? RevokedByIp { get; set; }

    [MaxLength(128)]
    public string? ReasonRevoked { get; set; }

    [MaxLength(64)]
    public string? ReplacedByTokenHash { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;
}