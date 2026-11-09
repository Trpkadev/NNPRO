using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/auth")]
[Tags("Authentication")]
public class AuthController : ApiControllerBase {
    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointSummary("User login")]
    [EndpointDescription("Authenticates a user using email and password, returning a JWT access and refresh token.")]
    [ProducesResponseType<Wrapper<AuthResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto requestDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new AuthResponseDto("access_token_dummy", "refresh_token_dummy", DateTimeOffset.UtcNow.AddMinutes(5)));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EndpointSummary("Register a new user account")]
    [ProducesResponseType<Wrapper<AuthResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto registerRequest, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new AuthResponseDto("access_token_dummy", "refresh_token_dummy", DateTimeOffset.UtcNow.AddMinutes(5)));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EndpointSummary("Refresh authentication access token")]
    [EndpointDescription("Issues a new access and refresh token pair using a valid refresh token.")]
    [ProducesResponseType<Wrapper<AuthResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequest, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Ok(new AuthResponseDto("access_token_dummy", "refresh_token_dummy", DateTimeOffset.UtcNow.AddMinutes(5)));
    }

    [HttpPost("logout")]
    //[Authorize]
    [EndpointSummary("Logout user and revoke refresh token")]
    [EndpointDescription("Revokes the current refresh token and invalidates the user session.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Logout([FromBody] LogoutRequestDto logoutRequest, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return NoContent();
    }
}