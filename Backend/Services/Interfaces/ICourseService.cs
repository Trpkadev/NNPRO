using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface ICourseService {
    Task<CourseResponseDto> GetByIdAsync(uint id, CancellationToken ct);

    Task<(CourseResponseDto[] Courses, Metadata Metadata)> GetAsync(RequestQuery requestDto, CancellationToken ct);

    Task<uint> CreateAsync(uint requestingUserId, CourseCreateRequestDto requestDto, CancellationToken ct);

    Task RemoveAsync(uint requestingUserId, uint id, CancellationToken ct);

    Task UpdateAsync(uint requestingUserId, uint id, CourseUpdateRequestDto requestDto, CancellationToken ct);

    Task<(CourseThumbnailResponseDto[]Courses, Metadata Metadata)> GetCourseThumbnailsAsync(CourseThumbnailRequestDto requestDto, CancellationToken ct);

    Task<CourseInstructorResponseDto[]> GetInstructorsAsync(uint courseId, CancellationToken ct);

    Task<SectionResponseDto[]> GetSectionsAsync(uint courseId, CancellationToken ct);
}