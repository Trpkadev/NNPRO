using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface ICourseService {
    Task<CourseResponseDto> GetByIdAsync(uint id, CancellationToken ct);

    Task<(CourseResponseDto[] Courses, Metadata Metadata)> GetAsync(RequestQuery requestDto, CancellationToken ct);

    Task<uint> CreateAsync(CourseCreateRequestDto requestDto, CancellationToken ct);

    Task RemoveAsync(uint id, CancellationToken ct);

    Task UpdateAsync(uint id, CourseUpdateRequestDto requestDto, CancellationToken ct);

    Task UpdateInstructorsAsync(uint id, CourseUpdateInstructorsRequestDto requestDto, CancellationToken ct);

    Task<(CourseThumbnailResponseDto[]Courses, Metadata Metadata)> GetCourseThumbnailsAsync(CourseThumbnailRequestDto requestDto, CancellationToken ct);

    Task<(CourseInstructorResponseDto[] Instructors, Metadata Metadata)> GetInstructorsAsync(uint id, RequestQuery request, CancellationToken ct);

    Task<(SectionResponseDto[] Sections, Metadata Metadata)> GetSectionsAsync(uint id, RequestQuery request, CancellationToken ct);

    Task<(CourseResponseDto[] Courses, Metadata Metadata)> GetManageableCoursesAsync(RequestQuery request, CancellationToken ct);
}