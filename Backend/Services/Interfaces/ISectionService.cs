using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface ISectionService {
    Task<SectionResponseDto> GetByIdAsync(uint requestingUserId, uint sectionId, CancellationToken ct);

    Task<(LessonResponseWrapperDto Lessons, Metadata Metadata)> GetLessonsAsync(uint sectionId, uint userId, RequestQuery request, CancellationToken ct);

    Task<(QuizResponseDto[] Quizzes, Metadata Metadata)> GetQuizzesAsync(uint sectionId, uint userId, RequestQuery request, CancellationToken ct);

    Task<uint> CreateAsync(uint requestingUserId, CreateSectionRequestDto request, CancellationToken ct);

    Task UpdateAsync(uint requestingUserId, uint sectionId, CreateSectionRequestDto request, CancellationToken ct);

    Task DeleteAsync(uint requestingUserId, uint sectionId, CancellationToken ct);
}