using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface ICourseService {
    Task<CourseResponseDto> GetByIdAsync(uint id, uint userId, CancellationToken ct);

    Task<(CourseResponseDto[] Courses, Metadata Metadata)> GetAsync(RequestQuery requestDto, CancellationToken ct);

    Task<uint> CreateAsync(uint userId, CourseCreateRequestDto requestDto, CancellationToken ct);

    Task RemoveAsync(uint userId, uint id, CancellationToken ct);

    Task UpdateAsync(uint userId, uint id, CourseUpdateRequestDto requestDto, CancellationToken ct);

    Task<(CourseThumbnailResponseDto[]Courses, Metadata Metadata)> GetCourseThumbnailsAsync(uint userId, CourseThumbnailRequestDto requestDto, CancellationToken ct);

    Task<(CourseInstructorResponseDto[] Instructors, Metadata Metadata)> GetInstructorsAsync(uint courseId, RequestQuery request, CancellationToken ct);

    Task<(SectionResponseDto[] Sections, Metadata Metadata)> GetSectionsAsync(uint courseId, uint userId, RequestQuery request, CancellationToken ct);

    Task<(CourseResponseDto[] Courses, Metadata Metadata)> GetManageableCoursesAsync(uint userId, RequestQuery request, CancellationToken ct);
}