namespace Backend.Models.DTOs.Response;

public sealed record AdminSessionResponseDto(
    Guid SessionId,
    string? DeviceName,
    string? UserAgent,
    string? CreatedByIp,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc);