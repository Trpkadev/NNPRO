using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface ILessonService {
    Task<LessonResponseDto> GetByIdAsync(uint id, CancellationToken ct);

    Task<AttachmentResponseDto[]> GetAttachmentsAsync(uint lessonId, CancellationToken ct);

    Task<uint> CreateAsync(uint requestingUserId, CreateLessonRequestDto request, CancellationToken ct);

    Task UpdateAsync(uint requestingUserId, uint lessonId, CreateLessonRequestDto request, CancellationToken ct);

    Task DeleteAsync(uint requestingUserId, uint lessonId, CancellationToken ct);

    Task UpdateProgressAsync(uint studentId, uint lessonId, UpdateProgressDto request, CancellationToken ct);

    Task<uint> AddAttachmentAsync(uint requestingUserId, AttachmentRequestDto request, CancellationToken ct);

    Task DeleteAttachmentAsync(uint requestingUserId, uint lessonId, uint attachmentId, CancellationToken ct);
}