using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;

namespace Backend.Services;

public class LessonService : ILessonService {
    public Task<LessonResponseDto[]?> GetByIdAsync(uint id, CancellationToken ct) => throw new NotImplementedException();
}