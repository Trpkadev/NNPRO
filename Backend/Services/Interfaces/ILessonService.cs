using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;

namespace Backend.Services.Interfaces;

public interface ILessonService {
    Task<LessonResponseDto> GetByIdAsync(uint id, CancellationToken ct);

    Task<(AttachmentResponseDto[] Attachments, Metadata Metadata)> GetAttachmentsAsync(uint lessonId, RequestQuery request, CancellationToken ct);

    Task<uint> CreateAsync(CreateLessonRequestDto request, CancellationToken ct);

    Task UpdateAsync(uint lessonId, CreateLessonRequestDto request, CancellationToken ct);

    Task DeleteAsync(uint lessonId, CancellationToken ct);

    Task UpdateProgressAsync(uint lessonId, UpdateProgressDto request, CancellationToken ct);

    Task<uint> AddAttachmentAsync(AttachmentRequestDto request, CancellationToken ct);

    Task DeleteAttachmentAsync(uint lessonId, uint attachmentId, CancellationToken ct);
}