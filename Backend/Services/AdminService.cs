using Backend.Config;
using Backend.Data;
using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class AdminService(AppDbContext dbContext, IAuthService authService, ITokenBlacklistService tokenBlacklistService, IOptions<JwtOptions> jwtOptions, IOptions<DatabaseLimitsOptions> databaseLimitsOptions) : IAdminService {
    public async Task<(UserResponseDto[] Users, Metadata Metadata)> GetUsersAsync(RequestQuery query, CancellationToken ct)
    {
        var usersQuery = dbContext.Users.AsNoTracking()
            .Filter(query.Filters)
            .OrderByProperty(query.Sorting);
        var (users, pagination) = await usersQuery.ToPagedResultAsync(query, databaseLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        UserResponseDto[] userResponses = [.. users.Select(user => new UserResponseDto(user.Id, user.Username, user.Email, user.Role.ToString()))];
        return (userResponses, new Metadata(pagination, query));
    }

    public Task<AdminUserDetailsDto?> GetUserAsync(uint userId, CancellationToken ct) =>
        dbContext.Users.AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new AdminUserDetailsDto(user.Id, user.Username, user.Email, user.Role.ToString(), user.Enrollments.Count, user.LessonProgresses.Count, user.QuizAttempts.Count, user.RefreshTokens.Count(token => token.RevokedAtUtc == null && token.ExpiresAtUtc > DateTime.UtcNow)))
            .FirstOrDefaultAsync(ct);

    public async Task<bool> UpdateUserRoleAsync(uint userId, UserRole role, string? adminIp, CancellationToken ct)
    {
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role), role, null);

        var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Id == userId, ct);
        if (user is null)
            return false;
        if (user.Role == role)
            return true;

        var nowUtc = DateTime.UtcNow;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        user.Role = role;
        await dbContext.RefreshTokens.Where(token => token.UserId == userId && token.RevokedAtUtc == null && token.ExpiresAtUtc > nowUtc)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(token => token.RevokedAtUtc, nowUtc)
                    .SetProperty(token => token.RevokedByIp, adminIp)
                    .SetProperty(token => token.ReasonRevoked, "User role changed"),
                ct);

        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        await BlacklistUnexpiredAccessTokensAsync(userId, nowUtc, ct);
        return true;
    }

    public async Task<(AdminSessionResponseDto[] Sessions, Metadata Metadata)?> GetSessionsAsync(uint userId, RequestQuery query, CancellationToken ct)
    {
        bool userExists = await dbContext.Users.AsNoTracking().AnyAsync(user => user.Id == userId, ct);
        if (!userExists)
            return null;

        var nowUtc = DateTime.UtcNow;
        bool useDefaultSort = string.IsNullOrWhiteSpace(query.SortBy) || string.Equals(query.SortBy, "Id", StringComparison.OrdinalIgnoreCase);
        string sortBy = (useDefaultSort ? nameof(AdminSessionResponseDto.CreatedAtUtc) : query.SortBy) ?? "Id";
        bool descending = useDefaultSort || (query.Descending ?? false);

        var effectiveQuery = new RequestQuery {
            Page = query.Page,
            PageSize = query.PageSize,
            SortBy = sortBy,
            Descending = descending,
            Filters = query.Filters
        };
        var filters = query.Filters.ToDictionary(filter => string.Equals(filter.Key, nameof(AdminSessionResponseDto.SessionId), StringComparison.OrdinalIgnoreCase) ? nameof(RefreshToken.TokenFamilyId) : filter.Key, filter => filter.Value, StringComparer.OrdinalIgnoreCase);
        string entitySortBy = string.Equals(sortBy, nameof(AdminSessionResponseDto.SessionId), StringComparison.OrdinalIgnoreCase) ? nameof(RefreshToken.TokenFamilyId) : sortBy;
        var sessionsQuery = dbContext.RefreshTokens.AsNoTracking()
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null && token.ExpiresAtUtc > nowUtc)
            .Filter(filters)
            .OrderByProperty(new SortingMetadata { SortBy = entitySortBy, IsDescending = descending });
        var (sessions, pagination) = await sessionsQuery.ToPagedResultAsync(effectiveQuery, databaseLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        AdminSessionResponseDto[] sessionResponses = [.. sessions.Select(token => new AdminSessionResponseDto(token.TokenFamilyId, token.DeviceName, token.UserAgent, token.CreatedByIp, token.CreatedAtUtc, token.ExpiresAtUtc))];
        return (sessionResponses, new Metadata(pagination, effectiveQuery));
    }

    public Task<bool> RevokeSessionAsync(uint userId, Guid sessionId, string? adminIp, CancellationToken ct) => authService.RevokeSessionFamilyAsync(userId, sessionId, adminIp, "Revoked by administrator", ct);

    public Task<bool> RevokeAllSessionsAsync(uint userId, string? adminIp, CancellationToken ct) => authService.RevokeAllSessionsAsync(userId, adminIp, ct);

    private async Task BlacklistUnexpiredAccessTokensAsync(uint userId, DateTime nowUtc, CancellationToken ct)
    {
        var accessTokenCutoffUtc = nowUtc.AddMinutes(-jwtOptions.Value.AccessTokenExpirationMinutes);
        var accessTokens = await dbContext.RefreshTokens.AsNoTracking()
            .Where(token => token.UserId == userId && token.CreatedAtUtc > accessTokenCutoffUtc)
            .Select(token => new { token.JwtId, token.CreatedAtUtc })
            .ToListAsync(ct);

        foreach (var token in accessTokens.DistinctBy(token => token.JwtId))
            await tokenBlacklistService.BlacklistTokenAsync(token.JwtId, token.CreatedAtUtc.AddMinutes(jwtOptions.Value.AccessTokenExpirationMinutes) - nowUtc, ct);
    }
}