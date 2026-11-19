using Backend.Configuration;
using Backend.Data;
using Backend.Exceptions;
using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class AdminService(ServiceContext<AdminService> ctx, IAuthService authService) : IAdminService {
    public async Task<AdminUserDetailsDto> GetUserAsync(uint userId, CancellationToken ct)
    {
        var user = await ctx.Db.Users.AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new AdminUserDetailsDto(user.Id, user.Username, user.Email, user.Role.ToString(), user.Enrollments.Count, user.LessonProgresses.Count, user.QuizAttempts.Count, user.RefreshTokens.Count(token => token.RevokedAtUtc == null && token.ExpiresAtUtc > DateTime.UtcNow)))
            .FirstOrDefaultAsync(ct);
        return user ?? throw ServiceException.NotFound(Resources.ErrorUserNotFound);
    }

    public async Task UpdateUserRoleAsync(uint userId, UserRole role, string? adminIp, CancellationToken ct)
    {
        if (!Enum.IsDefined(role))
            throw ServiceException.BadRequest(Resources.ErrorInvalidUserRole);

        var user = await ctx.Db.Users.FirstOrDefaultAsync(candidate => candidate.Id == userId, ct);
        if (user is null)
            throw ServiceException.NotFound(Resources.ErrorUserNotFound);

        if (user.Role == role)
            return;

        var nowUtc = DateTime.UtcNow;
        await using var transaction = await ctx.Db.Database.BeginTransactionAsync(ct);

        user.Role = role;
        await ctx.Db.RefreshTokens.Where(token => token.UserId == userId && token.RevokedAtUtc == null && token.ExpiresAtUtc > nowUtc)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(token => token.RevokedAtUtc, nowUtc)
                    .SetProperty(token => token.RevokedByIp, adminIp)
                    .SetProperty(token => token.ReasonRevoked, "User role changed"),
                ct);

        await ctx.Db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        await BlacklistUnexpiredAccessTokensAsync(userId, nowUtc, ct);
    }

    public async Task<(AdminSessionResponseDto[] Sessions, Metadata Metadata)> GetSessionsAsync(uint userId, RequestQuery query, CancellationToken ct)
    {
        bool userExists = await ctx.Db.Users.AsNoTracking().AnyAsync(user => user.Id == userId, ct);
        if (!userExists)
            throw ServiceException.NotFound(Resources.ErrorUserNotFound);

        var nowUtc = DateTime.UtcNow;
        bool useDefaultSort = string.IsNullOrWhiteSpace(query.SortBy) || string.Equals(query.SortBy, "Id", StringComparison.OrdinalIgnoreCase);
        string sortBy = (useDefaultSort ? nameof(AdminSessionResponseDto.CreatedAtUtc) : query.SortBy) ?? "Id";
        bool descending = useDefaultSort || (query.Descending ?? false);
        var filters = query.Filters.ToDictionary(filter => string.Equals(filter.Key, nameof(AdminSessionResponseDto.SessionId), StringComparison.OrdinalIgnoreCase) ? nameof(RefreshToken.TokenFamilyId) : filter.Key, filter => filter.Value, StringComparer.OrdinalIgnoreCase);

        query = new RequestQuery {
            Page = query.Page,
            PageSize = query.PageSize,
            SortBy = sortBy,
            Descending = descending,
            Filters = filters
        };
        string entitySortBy = string.Equals(sortBy, nameof(AdminSessionResponseDto.SessionId), StringComparison.OrdinalIgnoreCase) ? nameof(RefreshToken.TokenFamilyId) : sortBy;
        var sessionsQuery = ctx.Db.RefreshTokens.AsNoTracking()
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null && token.ExpiresAtUtc > nowUtc)
            .Filter(filters)
            .OrderByProperty(new SortingMetadata { SortBy = entitySortBy, IsDescending = descending });
        var (sessions, pagination) = await sessionsQuery.ToPagedResultAsync(query, ctx.MaxUnpagedItems, ct);

        AdminSessionResponseDto[] sessionResponses = [.. sessions.Select(token => new AdminSessionResponseDto(token.TokenFamilyId, token.DeviceName, token.UserAgent, token.CreatedByIp, token.CreatedAtUtc, token.ExpiresAtUtc))];
        return (sessionResponses, new Metadata(pagination, query));
    }

    public async Task RevokeSessionAsync(uint userId, Guid sessionId, string? adminIp, CancellationToken ct)
    {
        await EnsureUserExistsAsync(userId, ct);
        bool revoked = await authService.RevokeSessionFamilyAsync(userId, sessionId, adminIp, "Revoked by administrator", ct);
        if (!revoked)
            throw ServiceException.NotFound(Resources.ErrorSessionNotFound);
    }

    public async Task RevokeAllSessionsAsync(uint userId, string? adminIp, CancellationToken ct)
    {
        await EnsureUserExistsAsync(userId, ct);
        await authService.RevokeAllSessionsAsync(userId, adminIp, ct);
    }

    private async Task EnsureUserExistsAsync(uint userId, CancellationToken ct)
    {
        if (!await ctx.Db.Users.AsNoTracking().AnyAsync(user => user.Id == userId, ct))
            throw ServiceException.NotFound(Resources.ErrorUserNotFound);
    }

    private async Task BlacklistUnexpiredAccessTokensAsync(uint userId, DateTime nowUtc, CancellationToken ct)
    {
        var accessTokenCutoffUtc = nowUtc.AddMinutes(-ctx.JwtOptions.Value.AccessTokenExpirationMinutes);
        var accessTokens = await ctx.Db.RefreshTokens.AsNoTracking()
            .Where(token => token.UserId == userId && (token.AccessTokenExpiresAtUtc > nowUtc || token.AccessTokenExpiresAtUtc == null && token.CreatedAtUtc > accessTokenCutoffUtc))
            .Select(token => new { token.JwtId, token.CreatedAtUtc, token.AccessTokenExpiresAtUtc })
            .ToListAsync(ct);

        foreach (var token in accessTokens.DistinctBy(token => token.JwtId))
        {
            var expiresAtUtc = token.AccessTokenExpiresAtUtc ?? token.CreatedAtUtc.AddMinutes(ctx.JwtOptions.Value.AccessTokenExpirationMinutes);
            await ctx.TokenBlacklistService.BlacklistTokenAsync(token.JwtId, expiresAtUtc - nowUtc, ct);
        }
    }
}