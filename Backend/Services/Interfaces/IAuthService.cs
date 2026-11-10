using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface IAuthService {
    Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request, string? clientIp, string? userAgent, CancellationToken ct);

    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, string? clientIp, string? userAgent, uint? currentUserId, string? currentJwtId, CancellationToken ct);

    Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request, string? clientIp, CancellationToken ct);

    Task<bool> RevokeSessionByRefreshTokenAsync(string refreshToken, string? clientIp, CancellationToken ct);

    Task<bool> RevokeCurrentSessionAsync(uint userId, string jwtId, string? clientIp, CancellationToken ct);

    Task<bool> RevokeSessionFamilyAsync(uint userId, Guid tokenFamilyId, string? clientIp, string reason, CancellationToken ct);

    Task<bool> RevokeAllSessionsAsync(uint userId, string? clientIp, CancellationToken ct);
}