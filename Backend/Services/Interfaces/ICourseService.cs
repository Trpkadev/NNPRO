using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface ICourseService {
    Task<CourseResponseDto?> GetByIdAsync(uint id, CancellationToken ct);

    Task<(CourseResponseDto[] Courses, Metadata Metadata)> GetAsync(RequestQuery requestDto, CancellationToken ct);

    Task<uint> CreateAsync(CourseCreateRequestDto requestDto, CancellationToken ct);

    Task<bool> RemoveAsync(uint id, CancellationToken ct);

    Task<bool> UpdateAsync(uint id, CourseUpdateRequestDto requestDto, CancellationToken ct);

    Task<(CourseThumbnailResponseDto[]Courses, Metadata Metadata)> GetCourseThumbnailsAsync(CourseThumbnailRequestDto requestDto, CancellationToken ct);
}