using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface IUserService {
    Task<(UserResponseDto[] Users, Metadata Metadata)> GetUsersAsync(RequestQuery request, CancellationToken ct);

    Task<(UserCourseResponseDto[] Courses, Metadata Metadata)> GetMyCoursesAsync(RequestQuery request, CancellationToken ct);

    Task<(UserLessonProgressResponseDto[] Progress, Metadata Metadata)> GetMyProgressAsync(RequestQuery request, CancellationToken ct);

    Task UpdateAsync(UpdateUserRequestDto request, CancellationToken ct);

    Task UpdateAsync(uint userId, UpdateUserRequestDto request, CancellationToken ct);

    Task DeleteAsync(CancellationToken ct);

    Task DeleteAsync(uint userId, CancellationToken ct);
}