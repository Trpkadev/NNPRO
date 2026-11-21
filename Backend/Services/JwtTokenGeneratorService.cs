using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend.Configuration;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public class JwtTokenGenerator(IOptions<JwtOptions> jwtOptions) : IJwtTokenGenerator {
    public (string AccessToken, string JwtId, DateTime ExpiresAtUtc) GenerateAccessToken(User user)
    {
        var options = jwtOptions.Value;
        byte[] key = GetSigningKey(options);
        string jwtId = Guid.NewGuid().ToString("N");
        var issuedAtUtc = DateTime.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddMinutes(options.AccessTokenExpirationMinutes);

        Claim[] claims = [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(issuedAtUtc).ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),
            new("name", user.Username),
            new("role", user.Role.ToString())
        ];

        var token = new JwtSecurityToken(options.Issuer, options.Audience, claims, expires: expiresAtUtc, signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), jwtId, expiresAtUtc);
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)).Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static byte[] GetSigningKey(JwtOptions options)
    {
        byte[] key = Encoding.UTF8.GetBytes(options.SecretKey);
        return key.Length < 32 ? throw new InvalidOperationException(Resources.ErrorJwtSigningKeyTooShort) : key;
    }
}