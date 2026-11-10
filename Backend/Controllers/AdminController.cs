using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Authorize(Roles = "Admin")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[Route("api/admin")]
[Tags("Administration")]
public class AdminController(IAdminService adminService) : ApiControllerBase {
    [HttpGet("users")]
    [EndpointSummary("List users")]
    [EndpointDescription("Returns a paginated, filterable list of users without sensitive account data.")]
    [ProducesResponseType<Wrapper<UserResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsers([FromQuery] RequestQuery query, CancellationToken cancellationToken)
    {
        var (users, metadata) = await adminService.GetUsersAsync(query, cancellationToken);
        return Ok(users, metadata);
    }

    [HttpGet("users/{userId:uint}")]
    [EndpointSummary("Get user details")]
    [ProducesResponseType<Wrapper<AdminUserDetailsDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(uint userId, CancellationToken cancellationToken)
    {
        var user = await adminService.GetUserAsync(userId, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("users/{userId:uint}/role")]
    [EndpointSummary("Update a user's role")]
    [EndpointDescription("Changing a user's role revokes their active refresh tokens and invalidates their outstanding access tokens.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRole(uint userId, [FromBody] UpdateUserRoleRequestDto request, CancellationToken cancellationToken)
    {
        bool userExists = await adminService.UpdateUserRoleAsync(userId, request.Role, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);

        return userExists ? NoContent() : NotFound();
    }

    [HttpGet("users/{userId:uint}/sessions")]
    [EndpointSummary("List a user's active sessions")]
    [ProducesResponseType<Wrapper<AdminSessionResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessions(uint userId, [FromQuery] RequestQuery query, CancellationToken cancellationToken)
    {
        var result = await adminService.GetSessionsAsync(userId, query, cancellationToken);
        return result is null ? NotFound() : Ok(result.Value.Sessions, result.Value.Metadata);
    }

    [HttpDelete("users/{userId:uint}/sessions/{sessionId:guid}")]
    [EndpointSummary("Revoke one user session")]
    [EndpointDescription("Revokes the refresh-token family and invalidates all unexpired access tokens associated with the session.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession(uint userId, Guid sessionId, CancellationToken cancellationToken)
    {
        bool sessionExists = await adminService.RevokeSessionAsync(userId, sessionId, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);

        return sessionExists ? NoContent() : NotFound();
    }

    [HttpDelete("users/{userId:uint}/sessions")]
    [EndpointSummary("Revoke all sessions for a user")]
    [EndpointDescription("Revokes all refresh tokens and invalidates outstanding access tokens for the specified user.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeAllSessions(uint userId, CancellationToken cancellationToken)
    {
        bool userExists = await adminService.RevokeAllSessionsAsync(userId, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);

        return userExists ? NoContent() : NotFound();
    }
}