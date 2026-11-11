using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;

namespace Backend.Services.Interfaces;

public interface IAdminService {
    Task<(UserResponseDto[] Users, Metadata Metadata)> GetUsersAsync(RequestQuery query, CancellationToken ct);

    Task<AdminUserDetailsDto?> GetUserAsync(uint userId, CancellationToken ct);

    Task<bool> UpdateUserRoleAsync(uint userId, UserRole role, string? adminIp, CancellationToken ct);

    Task<(AdminSessionResponseDto[] Sessions, Metadata Metadata)?> GetSessionsAsync(uint userId, RequestQuery query, CancellationToken ct);

    Task<bool> RevokeSessionAsync(uint userId, Guid sessionId, string? adminIp, CancellationToken ct);

    Task<bool> RevokeAllSessionsAsync(uint userId, string? adminIp, CancellationToken ct);
}