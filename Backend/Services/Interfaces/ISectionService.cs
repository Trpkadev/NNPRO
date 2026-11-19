using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface ISectionService {
    Task<SectionResponseDto> GetByIdAsync(uint requestingUserId, uint sectionId, CancellationToken ct);

    Task<LessonResponseDto[]> GetLessonsAsync(uint sectionId, CancellationToken ct);

    Task<QuizResponseDto[]> GetQuizzesAsync(uint sectionId, CancellationToken ct);

    Task<uint> CreateAsync(uint requestingUserId, CreateSectionRequestDto request, CancellationToken ct);

    Task UpdateAsync(uint requestingUserId, uint sectionId, CreateSectionRequestDto request, CancellationToken ct);

    Task DeleteAsync(uint requestingUserId, uint sectionId, CancellationToken ct);
}