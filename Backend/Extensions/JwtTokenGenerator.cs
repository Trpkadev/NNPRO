using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend.Config;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Extensions;

public class JwtTokenGenerator(IOptions<JwtOptions> jwtOptions) : IJwtTokenGenerator {
    public (string AccessToken, string JwtId, DateTime ExpiresAtUtc) GenerateAccessToken(User user)
    {
        var options = jwtOptions.Value;
        byte[] key = GetSigningKey(options);
        string jwtId = Guid.NewGuid().ToString("N");
        var issuedAtUtc = DateTime.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddMinutes(options.AccessTokenExpirationMinutes);
        Claim[] claims = [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(issuedAtUtc).ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),
            new("name", user.Username),
            new("role", user.Role.ToString())
        ];

        var token = new JwtSecurityToken(options.Issuer, options.Audience, claims, expires: expiresAtUtc, signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), jwtId, expiresAtUtc);
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)).Replace('+', '-').Replace('/', '_').TrimEnd('=');

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var options = jwtOptions.Value;
        var validationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(GetSigningKey(options)),
            ValidateLifetime = false,
            RequireExpirationTime = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = "name",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            return new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(token, validationParameters, out _);
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static byte[] GetSigningKey(JwtOptions options)
    {
        byte[] key = Encoding.UTF8.GetBytes(options.SecretKey);
        return key.Length < 32 ? throw new InvalidOperationException(Resources.ErrorJwtSigningKeyTooShort) : key;
    }
}