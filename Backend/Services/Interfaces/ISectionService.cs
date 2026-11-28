using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface ISectionService {
    Task<SectionResponseDto> GetByIdAsync(uint sectionId, CancellationToken ct);

    Task<(LessonResponseWrapperDto Lessons, Metadata Metadata)> GetLessonsAsync(uint sectionId, RequestQuery request, CancellationToken ct);

    Task<(QuizResponseDto[] Quizzes, Metadata Metadata)> GetQuizzesAsync(uint sectionId, RequestQuery request, CancellationToken ct);

    Task<uint> CreateAsync(CreateSectionRequestDto request, CancellationToken ct);

    Task UpdateAsync(uint sectionId, CreateSectionRequestDto request, CancellationToken ct);

    Task DeleteAsync(uint sectionId, CancellationToken ct);
}