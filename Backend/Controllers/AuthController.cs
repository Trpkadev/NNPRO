using Backend.Config;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Backend.Controllers;

[Route("api/auth")]
[Tags("Authentication")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
public class AuthController(IAuthService authService, IOptions<JwtOptions> jwtOptions, IConfiguration configuration, IWebHostEnvironment environment) : ApiControllerBase {
    private const string RefreshCookieName = "refreshToken";
    private const string RefreshCookiePath = "/api/auth";

    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointSummary("User login")]
    [EndpointDescription("Authenticates using a username or email and password. Returns an access token and sets the HttpOnly refreshToken cookie. If an Origin header is sent, it must be allowed.")]
    [ProducesResponseType<Wrapper<AuthResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto requestDto, CancellationToken cancellationToken)
    {
        if (!IsAllowedOrigin())
            return Forbid();

        uint? currentUserId = null;
        string? currentJwtId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            if (uint.TryParse(User.FindFirst("sub")?.Value, out uint parsedUserId))
                currentUserId = parsedUserId;
            currentJwtId = User.FindFirst("jti")?.Value;
        }

        var response = await authService.LoginAsync(requestDto, GetClientIp(), GetUserAgent(), currentUserId, currentJwtId, cancellationToken);
        if (response is null)
            return Unauthorized();

        SetRefreshTokenCookie(response.RefreshToken);
        return Ok(response);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EndpointSummary("Register a new user account")]
    [EndpointDescription("Creates an account, returns an access token and sets the HttpOnly refreshToken cookie. If an Origin header is sent, it must be allowed.")]
    [ProducesResponseType<Wrapper<AuthResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto registerRequest, CancellationToken cancellationToken)
    {
        if (!IsAllowedOrigin())
            return Forbid();

        var response = await authService.RegisterAsync(registerRequest, GetClientIp(), GetUserAgent(), cancellationToken);
        if (response is null)
            return Conflict();

        SetRefreshTokenCookie(response.RefreshToken);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EndpointSummary("Refresh authentication access token")]
    [EndpointDescription("Requires the HttpOnly refreshToken cookie and an allowed Origin header. Issues a new access token and rotates the cookie.")]
    [ProducesResponseType<Wrapper<AuthResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(CancellationToken cancellationToken)
    {
        if (!IsAllowedOrigin(requireOrigin: true))
            return Forbid();

        if (!Request.Cookies.TryGetValue(RefreshCookieName, out string? refreshToken) || string.IsNullOrWhiteSpace(refreshToken) || refreshToken.Length > 128)
            return Unauthorized();

        var refreshTokenRequest = new RefreshTokenRequestDto(refreshToken);
        var response = await authService.RefreshTokenAsync(refreshTokenRequest, GetClientIp(), cancellationToken);
        if (response is null)
        {
            DeleteRefreshTokenCookie();
            return Unauthorized();
        }

        SetRefreshTokenCookie(response.RefreshToken);
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    [EndpointSummary("Logout user and revoke refresh token")]
    [EndpointDescription("Requires a Bearer access token. Revokes the current session and expires the refreshToken cookie. If an Origin header is sent, it must be allowed.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!IsAllowedOrigin())
            return Forbid();

        string? subject = User.FindFirst("sub")?.Value;
        string? jwtId = User.FindFirst("jti")?.Value;
        if (!uint.TryParse(subject, out uint userId) || string.IsNullOrEmpty(jwtId))
            return Unauthorized();

        bool revoked = await authService.RevokeCurrentSessionAsync(userId, jwtId, GetClientIp(), cancellationToken);
        if (!revoked)
            return Unauthorized();

        DeleteRefreshTokenCookie();
        return NoContent();
    }

    private void SetRefreshTokenCookie(string refreshToken) => Response.Cookies.Append(RefreshCookieName,
        refreshToken,
        new CookieOptions {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = RefreshCookiePath,
            MaxAge = TimeSpan.FromDays(jwtOptions.Value.RefreshTokenExpirationDays)
        });

    private void DeleteRefreshTokenCookie() => Response.Cookies.Delete(RefreshCookieName,
        new CookieOptions {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = RefreshCookiePath
        });

    private bool IsAllowedOrigin(bool requireOrigin = false)
    {
        string origin = Request.Headers.Origin.ToString();
        if (string.IsNullOrWhiteSpace(origin))
            return !requireOrigin;

        string requestOrigin = $"{Request.Scheme}://{Request.Host}";
        if (string.Equals(origin, requestOrigin, StringComparison.OrdinalIgnoreCase))
            return true;

        string[] allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (allowedOrigins.Length == 0 && environment.IsDevelopment())
            allowedOrigins = ["http://localhost:5173", "https://localhost:5173"];

        return allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }

    private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? GetUserAgent()
    {
        string userAgent = Request.Headers.UserAgent.ToString();
        return string.IsNullOrEmpty(userAgent) ? null : userAgent[..Math.Min(userAgent.Length, 512)];
    }
}