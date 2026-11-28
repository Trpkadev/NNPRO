using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface IEnrollmentService {
    Task<(UserResponseDto[] Users, Metadata Metadata)> GetEnrolledUsersAsync(uint id, RequestQuery request, CancellationToken ct);

    Task<uint> EnrollIntoCourseAsync(EnrollmentRequestDto request, CancellationToken ct);

    Task<uint> ForceEnrollIntoCourseAsync(ForceEnrollmentRequestDto request, CancellationToken ct);

    Task UnenrollAsync(uint courseId, CancellationToken ct);

    Task ForceUnenrollAsync(ForceEnrollmentRequestDto request, CancellationToken ct);
}