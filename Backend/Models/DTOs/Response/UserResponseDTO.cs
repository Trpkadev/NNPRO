namespace Backend.Models.DTOs.Response;

public record UserResponseDto(
    uint Id,
    string Username,
    string Email,
    string Role);