using System.Security.Claims;
using Backend.Models.Entities;
using Backend.Services.Interfaces;

namespace Backend.Services;

public class JwtTokenGenerator : IJwtTokenGenerator {
    public (string AccessToken, string JwtId, DateTime ExpiresAtUtc) GenerateAccessToken(User user) => throw new NotImplementedException();

    public string GenerateRefreshToken() => throw new NotImplementedException();

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token) => throw new NotImplementedException();
}