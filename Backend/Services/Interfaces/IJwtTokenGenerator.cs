using Backend.Models.Entities;

namespace Backend.Services.Interfaces;

public interface IJwtTokenGenerator {
    (string AccessToken, string JwtId, DateTime ExpiresAtUtc) GenerateAccessToken(User user);

    string GenerateRefreshToken();
}