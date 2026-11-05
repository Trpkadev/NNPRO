using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface ICourseService {
    Task<CourseDto?> GetByIdAsync(uint id, CancellationToken ct);

    Task<CourseThumbnailDto[]> GetCourseThumbnailsAsync(CourseThumbnailQuery query, CancellationToken ct);
}