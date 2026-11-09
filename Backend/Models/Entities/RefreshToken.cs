namespace Backend.Models.Entities;

public class RefreshToken {
    public uint Id { get; set; }

    public required string TokenHash { get; set; }
    public required string JwtId { get; set; }

    public uint UserId { get; set; }
    public User User { get; set; } = null!;

    public required DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReasonRevoked { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;
}