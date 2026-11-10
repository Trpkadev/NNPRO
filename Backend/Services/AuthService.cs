using System.Security.Cryptography;
using System.Text;
using Backend.Config;
using Backend.Data;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class AuthService(AppDbContext dbContext, IJwtTokenGenerator jwtTokenGenerator, ITokenBlacklistService tokenBlacklistService, IOptions<JwtOptions> jwtOptions) : IAuthService {
    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request, string? clientIp, string? userAgent, CancellationToken ct)
    {
        bool userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(user => user.Username == request.Username || user.Email == request.Email, ct);
        if (userExists)
            return null;

        User user = new() {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct);

        var (response, refreshToken) = CreateTokenPair(user, clientIp, userAgent, request.DeviceName, Guid.NewGuid());
        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(ct);
        return response;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, string? clientIp, string? userAgent, uint? currentUserId, string? currentJwtId, CancellationToken ct)
    {
        string? username = string.IsNullOrWhiteSpace(request.Username) ? null : request.Username;
        string? email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email;
        var user = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(candidate => username != null && candidate.Username == username || email != null && candidate.Email == email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var (response, refreshToken) = CreateTokenPair(user, clientIp, userAgent, request.DeviceName, Guid.NewGuid());
        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(ct);

        if (currentUserId.HasValue && !string.IsNullOrEmpty(currentJwtId))
            await RevokeCurrentSessionAsync(currentUserId.Value, currentJwtId, clientIp, ct);

        return response;
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request, string? clientIp, CancellationToken ct)
    {
        string presentedTokenHash = HashRefreshToken(request.RefreshToken);
        var currentToken = await dbContext.RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(token => token.TokenHash == presentedTokenHash, ct);

        if (currentToken is null)
            return null;

        string newRefreshTokenValue = jwtTokenGenerator.GenerateRefreshToken();
        string newRefreshTokenHash = HashRefreshToken(newRefreshTokenValue);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        var nowUtc = DateTime.UtcNow;
        int revokedCount = await dbContext.RefreshTokens.AsNoTracking()
            .Where(token => token.Id == currentToken.Id && token.RevokedAtUtc == null && token.ExpiresAtUtc > nowUtc)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(token => token.RevokedAtUtc, nowUtc)
                    .SetProperty(token => token.RevokedByIp, clientIp)
                    .SetProperty(token => token.ReasonRevoked, "Rotated")
                    .SetProperty(token => token.ReplacedByTokenHash, newRefreshTokenHash),
                ct);

        if (revokedCount == 0)
        {
            var latestToken = await dbContext.RefreshTokens.AsNoTracking()
                .FirstOrDefaultAsync(token => token.Id == currentToken.Id, ct);
            if (latestToken is null || !latestToken.IsRevoked)
                return null;

            var familyTokens = await dbContext.RefreshTokens.AsNoTracking()
                .Where(token => token.UserId == currentToken.UserId && token.TokenFamilyId == currentToken.TokenFamilyId)
                .ToListAsync(ct);
            await dbContext.RefreshTokens.Where(token => token.UserId == currentToken.UserId && token.TokenFamilyId == currentToken.TokenFamilyId && token.RevokedAtUtc == null)
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(token => token.RevokedAtUtc, nowUtc)
                        .SetProperty(token => token.RevokedByIp, clientIp)
                        .SetProperty(token => token.ReasonRevoked, "Refresh token reuse detected"),
                    ct);
            await transaction.CommitAsync(ct);
            await BlacklistAccessTokensAsync(familyTokens, ct);
            return null;
        }

        var user = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == currentToken.UserId, ct);
        if (user is null)
            return null;

        (string accessToken, string newJwtId, var expiresAtUtc) = jwtTokenGenerator.GenerateAccessToken(user);
        var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationDays);
        dbContext.RefreshTokens.Add(new RefreshToken {
            UserId = user.Id,
            JwtId = newJwtId,
            TokenHash = newRefreshTokenHash,
            TokenFamilyId = currentToken.TokenFamilyId,
            ExpiresAtUtc = refreshTokenExpiresAtUtc,
            AccessTokenExpiresAtUtc = expiresAtUtc,
            CreatedByIp = clientIp,
            UserAgent = currentToken.UserAgent,
            DeviceName = currentToken.DeviceName
        });
        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        await BlacklistAccessTokensAsync([currentToken], ct);

        return new AuthResponseDto(accessToken, newRefreshTokenValue, expiresAtUtc);
    }

    public async Task<bool> RevokeSessionByRefreshTokenAsync(string refreshToken, string? clientIp, CancellationToken ct)
    {
        string tokenHash = HashRefreshToken(refreshToken);
        var storedToken = await dbContext.RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, ct);

        return storedToken is not null && await RevokeSessionFamilyAsync(storedToken.UserId, storedToken.TokenFamilyId, clientIp, "Session revoked", ct);
    }

    public async Task<bool> RevokeCurrentSessionAsync(uint userId, string jwtId, string? clientIp, CancellationToken ct)
    {
        var refreshToken = await dbContext.RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(token => token.UserId == userId && token.JwtId == jwtId, ct);

        if (refreshToken is not null)
            return await RevokeSessionFamilyAsync(userId, refreshToken.TokenFamilyId, clientIp, "Session revoked", ct);

        await tokenBlacklistService.BlacklistTokenAsync(jwtId, TimeSpan.FromMinutes(jwtOptions.Value.AccessTokenExpirationMinutes), ct);
        return true;

    }

    public async Task<bool> RevokeSessionFamilyAsync(uint userId, Guid tokenFamilyId, string? clientIp, string reason, CancellationToken ct)
    {
        var familyTokens = await dbContext.RefreshTokens.AsNoTracking()
            .Where(token => token.UserId == userId && token.TokenFamilyId == tokenFamilyId)
            .ToListAsync(ct);
        if (familyTokens.Count == 0)
            return false;

        var nowUtc = DateTime.UtcNow;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        await dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.TokenFamilyId == tokenFamilyId && token.RevokedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(token => token.RevokedAtUtc, nowUtc)
                    .SetProperty(token => token.RevokedByIp, clientIp)
                    .SetProperty(token => token.ReasonRevoked, reason),
                ct);
        await transaction.CommitAsync(ct);

        await BlacklistAccessTokensAsync(familyTokens, ct);
        return true;
    }

    public async Task<bool> RevokeAllSessionsAsync(uint userId, string? clientIp, CancellationToken ct)
    {
        bool userExists = await dbContext.Users.AsNoTracking().AnyAsync(user => user.Id == userId, ct);
        if (!userExists)
            return false;

        var userTokens = await dbContext.RefreshTokens.AsNoTracking()
            .Where(token => token.UserId == userId)
            .ToListAsync(ct);
        var nowUtc = DateTime.UtcNow;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        await dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(token => token.RevokedAtUtc, nowUtc)
                    .SetProperty(token => token.RevokedByIp, clientIp)
                    .SetProperty(token => token.ReasonRevoked, "Revoked by administrator"),
                ct);
        await transaction.CommitAsync(ct);

        await BlacklistAccessTokensAsync(userTokens, ct);
        return true;
    }

    private (AuthResponseDto Response, RefreshToken RefreshToken) CreateTokenPair(User user, string? clientIp, string? userAgent, string? deviceName, Guid tokenFamilyId)
    {
        string refreshTokenValue = jwtTokenGenerator.GenerateRefreshToken();
        string tokenHash = HashRefreshToken(refreshTokenValue);
        (string accessToken, string jwtId, var expiresAtUtc) = jwtTokenGenerator.GenerateAccessToken(user);

        var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationDays);
        var refreshToken = new RefreshToken {
            UserId = user.Id,
            JwtId = jwtId,
            TokenHash = tokenHash,
            TokenFamilyId = tokenFamilyId,
            ExpiresAtUtc = refreshTokenExpiresAtUtc,
            AccessTokenExpiresAtUtc = expiresAtUtc,
            CreatedByIp = clientIp,
            UserAgent = userAgent,
            DeviceName = deviceName
        };
        return (new AuthResponseDto(accessToken, refreshTokenValue, expiresAtUtc), refreshToken);
    }

    private async Task BlacklistAccessTokensAsync(IEnumerable<RefreshToken> refreshTokens, CancellationToken ct)
    {
        foreach (var refreshToken in refreshTokens.DistinctBy(token => token.JwtId))
            await tokenBlacklistService.BlacklistTokenAsync(refreshToken.JwtId, GetAccessTokenRemainingLifetime(refreshToken), ct);
    }

    private TimeSpan GetAccessTokenRemainingLifetime(RefreshToken refreshToken) => (refreshToken.AccessTokenExpiresAtUtc ?? refreshToken.CreatedAtUtc.AddMinutes(jwtOptions.Value.AccessTokenExpirationMinutes)) - DateTime.UtcNow;

    private static string HashRefreshToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}