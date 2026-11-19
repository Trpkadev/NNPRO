using Backend.Data;
using Backend.Exceptions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class LessonService(AppDbContext dbContext) : ILessonService {
    public async Task<LessonResponseDto> GetByIdAsync(uint id, CancellationToken ct)
    {
        var lesson = await dbContext.Lessons.AsNoTracking()
            .Where(lesson => lesson.Id == id)
            .Select(lesson => new LessonResponseDto(lesson.Id, lesson.Title, lesson.Content, lesson.VideoUrl, lesson.Order, lesson.SectionId))
            .FirstOrDefaultAsync(ct);
        return lesson ?? throw ServiceException.NotFound(Resources.ErrorLessonNotFound);
    }

    public async Task<AttachmentResponseDto[]> GetAttachmentsAsync(uint lessonId, CancellationToken ct)
    {
        if (!await dbContext.Lessons.AnyAsync(lesson => lesson.Id == lessonId, ct))
            throw ServiceException.NotFound(Resources.ErrorLessonNotFound);

        return await dbContext.LessonAttachments.AsNoTracking()
            .Where(attachment => attachment.LessonId == lessonId)
            .OrderBy(attachment => attachment.Id)
            .Select(attachment => new AttachmentResponseDto(
                attachment.Id,
                attachment.FileName,
                attachment.Url,
                attachment.ContentType,
                attachment.SizeBytes,
                attachment.LessonId))
            .ToArrayAsync(ct);
    }

    public async Task<uint> CreateAsync(uint requestingUserId, CreateLessonRequestDto request, CancellationToken ct)
    {
        var section = await dbContext.Sections.AsNoTracking()
            .Where(candidate => candidate.Id == request.SectionId)
            .Select(candidate => new { candidate.Id, candidate.CourseId })
            .FirstOrDefaultAsync(ct);
        if (section is null)
            throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, section.CourseId, ct);

        Lesson lesson = new() {
            Title = request.Title,
            Content = request.Content,
            VideoUrl = request.VideoUrl,
            Order = request.Order,
            SectionId = request.SectionId
        };
        dbContext.Lessons.Add(lesson);
        await dbContext.SaveChangesAsync(ct);
        return lesson.Id;
    }

    public async Task UpdateAsync(uint requestingUserId, uint lessonId, CreateLessonRequestDto request, CancellationToken ct)
    {
        var lesson = await dbContext.Lessons.FirstOrDefaultAsync(candidate => candidate.Id == lessonId, ct);
        if (lesson is null)
            throw ServiceException.NotFound(Resources.ErrorLessonNotFound);

        await EnsureCanManageSectionAsync(requestingUserId, lesson.SectionId, ct);
        if (lesson.SectionId != request.SectionId)
            await EnsureCanManageSectionAsync(requestingUserId, request.SectionId, ct);

        lesson.Title = request.Title;
        lesson.Content = request.Content;
        lesson.VideoUrl = request.VideoUrl;
        lesson.Order = request.Order;
        lesson.SectionId = request.SectionId;
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(uint requestingUserId, uint lessonId, CancellationToken ct)
    {
        await EnsureCanManageLessonAsync(requestingUserId, lessonId, ct);
        await dbContext.Lessons.Where(lesson => lesson.Id == lessonId).ExecuteDeleteAsync(ct);
    }

    public async Task UpdateProgressAsync(uint studentId, uint lessonId, UpdateProgressDto request, CancellationToken ct)
    {
        var lesson = await dbContext.Lessons.AsNoTracking()
            .Where(candidate => candidate.Id == lessonId)
            .Select(candidate => new { candidate.Id, candidate.Section.CourseId })
            .FirstOrDefaultAsync(ct);
        if (lesson is null)
            throw ServiceException.NotFound(Resources.ErrorLessonNotFound);

        bool isEnrolled = await dbContext.Enrollments
            .AnyAsync(enrollment => enrollment.StudentId == studentId && enrollment.CourseId == lesson.CourseId, ct);
        if (!isEnrolled)
            throw ServiceException.Forbidden(Resources.ErrorCourseEnrollmentRequiredForProgress);

        var progress = await dbContext.StudentLessonProgresses
            .FirstOrDefaultAsync(candidate => candidate.StudentId == studentId && candidate.LessonId == lessonId, ct);
        DateTime? completedAtUtc = request.IsCompleted ? DateTime.UtcNow : null;
        if (progress is null)
        {
            dbContext.StudentLessonProgresses.Add(new StudentLessonProgress {
                StudentId = studentId,
                LessonId = lessonId,
                IsCompleted = request.IsCompleted,
                CompletedAtUtc = completedAtUtc
            });
        }
        else
        {
            progress.IsCompleted = request.IsCompleted;
            progress.CompletedAtUtc = completedAtUtc;
        }

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<uint> AddAttachmentAsync(uint requestingUserId, AttachmentRequestDto request, CancellationToken ct)
    {
        await EnsureCanManageLessonAsync(requestingUserId, request.LessonId, ct);
        LessonAttachment attachment = new() {
            FileName = request.FileName,
            Url = request.Url,
            ContentType = request.ContentType,
            SizeBytes = request.SizeBytes,
            LessonId = request.LessonId
        };
        dbContext.LessonAttachments.Add(attachment);
        await dbContext.SaveChangesAsync(ct);
        return attachment.Id;
    }

    public async Task DeleteAttachmentAsync(uint requestingUserId, uint lessonId, uint attachmentId, CancellationToken ct)
    {
        await EnsureCanManageLessonAsync(requestingUserId, lessonId, ct);
        int deleted = await dbContext.LessonAttachments
            .Where(attachment => attachment.Id == attachmentId && attachment.LessonId == lessonId)
            .ExecuteDeleteAsync(ct);
        if (deleted == 0)
            throw ServiceException.NotFound(Resources.ErrorLessonAttachmentNotFound);
    }

    private async Task EnsureCanManageLessonAsync(uint requestingUserId, uint lessonId, CancellationToken ct)
    {
        uint? courseId = await dbContext.Lessons.AsNoTracking()
            .Where(lesson => lesson.Id == lessonId)
            .Select(lesson => (uint?)lesson.Section.CourseId)
            .FirstOrDefaultAsync(ct);
        if (!courseId.HasValue)
            throw ServiceException.NotFound(Resources.ErrorLessonNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, courseId.Value, ct);
    }

    private async Task EnsureCanManageSectionAsync(uint requestingUserId, uint sectionId, CancellationToken ct)
    {
        uint? courseId = await dbContext.Sections.AsNoTracking()
            .Where(section => section.Id == sectionId)
            .Select(section => (uint?)section.CourseId)
            .FirstOrDefaultAsync(ct);
        if (!courseId.HasValue)
            throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, courseId.Value, ct);
    }
}