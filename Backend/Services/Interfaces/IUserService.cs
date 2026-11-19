using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface IUserService {
    Task<UserCourseResponseDto[]> GetMyCoursesAsync(uint userId, CancellationToken ct);

    Task<UserLessonProgressResponseDto[]> GetMyProgressAsync(uint userId, CancellationToken ct);

    Task UpdateAsync(uint userId, UpdateUserRequestDto request, CancellationToken ct);

    Task DeleteAsync(uint userId, CancellationToken ct);
}