using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface ILessonService {
    public Task<LessonResponseDto[]?> GetByIdAsync(uint id, CancellationToken ct);
}