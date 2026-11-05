using Backend.Models.DTOs;
using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;

namespace Backend.Services;

public class LessonService : ILessonService {
    public Task<LessonDto[]?> GetByIdAsync(uint id, CancellationToken ct) => throw new NotImplementedException();
}