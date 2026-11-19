using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface IEnrollmentService {
    Task<(UserResponseDto[] Users, Metadata Metadata)> GetEnrolledUsersAsync(uint courseId, uint requestingUserId, RequestQuery request, CancellationToken ct);

    Task<uint> EnrollIntoCourseAsync(uint studentId, EnrollmentRequestDto request, CancellationToken ct);

    Task<uint> ForceEnrollIntoCourseAsync(uint requestingUserId, ForceEnrollmentRequestDto request, CancellationToken ct);

    Task UnenrollAsync(uint studentId, uint courseId, CancellationToken ct);

    Task ForceUnenrollAsync(uint requestingUserId, uint courseId, uint studentId, CancellationToken ct);
}