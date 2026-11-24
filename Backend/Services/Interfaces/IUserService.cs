using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface IUserService {
    Task<(UserResponseDto[] Users, Metadata Metadata)> GetUsersAsync(uint userId, RequestQuery request, CancellationToken ct);

    Task<(UserCourseResponseDto[] Courses, Metadata Metadata)> GetMyCoursesAsync(uint userId, RequestQuery request, CancellationToken ct);

    Task<(UserLessonProgressResponseDto[] Progress, Metadata Metadata)> GetMyProgressAsync(uint userId, RequestQuery request, CancellationToken ct);

    Task UpdateAsync(uint userId, UpdateUserRequestDto request, CancellationToken ct);

    Task DeleteAsync(uint userId, CancellationToken ct);
}