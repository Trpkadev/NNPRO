using System.Text.Json.Serialization;

namespace Backend.Models.DTOs.Response;

public sealed record AuthResponseDto(
    string AccessToken,
    [property: JsonIgnore] string RefreshToken,
    DateTimeOffset ExpiresAtUtc,
    string TokenType = "Bearer");