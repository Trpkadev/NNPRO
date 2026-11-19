using Backend.Configuration;
using Backend.Data;
using Backend.Exceptions;
using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class LessonService(ServiceContext<LessonService> ctx) : ILessonService {
    public async Task<LessonResponseDto> GetByIdAsync(uint id, uint userId, CancellationToken ct)
    {
        var data = await ctx.Db.Lessons.AsNoTracking()
                       .Where(l => l.Id == id)
                       .Select(l => new {
                           l.Section.CourseId,
                           Dto = new LessonResponseDto(l.Id, l.Title, l.Content, l.VideoUrl, l.Order, l.SectionId)
                       })
                       .FirstOrDefaultAsync(ct)
                   ?? throw ServiceException.NotFound(Resources.ErrorLessonNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(ctx.Db, data.CourseId, userId, ct);
        return data.Dto;
    }

    public async Task<(AttachmentResponseDto[] Attachments, Metadata Metadata)> GetAttachmentsAsync(uint lessonId, uint userId, RequestQuery request, CancellationToken ct)
    {
        uint courseId = await ctx.Db.Lessons.AsNoTracking()
                            .Where(l => l.Id == lessonId)
                            .Select(l => (uint?)l.Section.CourseId)
                            .FirstOrDefaultAsync(ct)
                        ?? throw ServiceException.NotFound(Resources.ErrorLessonNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(ctx.Db, courseId, userId, ct);

        var (attachments, metadata) = await ctx.Db.LessonAttachments.AsNoTracking()
            .Where(attachment => attachment.LessonId == lessonId)
            .OrderBy(attachment => attachment.Id)
            .Select(attachment => new AttachmentResponseDto(
                attachment.Id,
                attachment.FileName,
                attachment.Url,
                attachment.ContentType,
                attachment.SizeBytes,
                attachment.LessonId))
            .ToPagedResultAsync(request, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        return (attachments, new Metadata(metadata, request));
    }

    public async Task<uint> CreateAsync(uint requestingUserId, CreateLessonRequestDto request, CancellationToken ct)
    {
        var section = await ctx.Db.Sections.AsNoTracking()
            .Where(candidate => candidate.Id == request.SectionId)
            .Select(candidate => new { candidate.Id, candidate.CourseId })
            .FirstOrDefaultAsync(ct);
        if (section is null)
            throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, requestingUserId, section.CourseId, ct);

        Lesson lesson = new() {
            Title = request.Title,
            Content = request.Content,
            VideoUrl = request.VideoUrl,
            Order = request.Order,
            SectionId = request.SectionId
        };
        ctx.Db.Lessons.Add(lesson);
        await ctx.Db.SaveChangesAsync(ct);
        return lesson.Id;
    }

    public async Task UpdateAsync(uint requestingUserId, uint lessonId, CreateLessonRequestDto request, CancellationToken ct)
    {
        var lesson = await ctx.Db.Lessons.FirstOrDefaultAsync(candidate => candidate.Id == lessonId, ct);
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
        await ctx.Db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(uint requestingUserId, uint lessonId, CancellationToken ct)
    {
        await EnsureCanManageLessonAsync(requestingUserId, lessonId, ct);
        await ctx.Db.Lessons.Where(lesson => lesson.Id == lessonId).ExecuteDeleteAsync(ct);
    }

    public async Task UpdateProgressAsync(uint studentId, uint lessonId, UpdateProgressDto request, CancellationToken ct)
    {
        var lesson = await ctx.Db.Lessons.AsNoTracking()
            .Where(candidate => candidate.Id == lessonId)
            .Select(candidate => new { candidate.Id, candidate.Section.CourseId })
            .FirstOrDefaultAsync(ct);
        if (lesson is null)
            throw ServiceException.NotFound(Resources.ErrorLessonNotFound);

        bool isEnrolled = await ctx.Db.Enrollments
            .AnyAsync(enrollment => enrollment.StudentId == studentId && enrollment.CourseId == lesson.CourseId, ct);
        if (!isEnrolled)
            throw ServiceException.Forbidden(Resources.ErrorCourseEnrollmentRequiredForProgress);

        var progress = await ctx.Db.StudentLessonProgresses
            .FirstOrDefaultAsync(candidate => candidate.StudentId == studentId && candidate.LessonId == lessonId, ct);
        DateTime? completedAtUtc = request.IsCompleted ? DateTime.UtcNow : null;
        if (progress is null)
        {
            ctx.Db.StudentLessonProgresses.Add(new StudentLessonProgress {
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

        await ctx.Db.SaveChangesAsync(ct);
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
        ctx.Db.LessonAttachments.Add(attachment);
        await ctx.Db.SaveChangesAsync(ct);
        return attachment.Id;
    }

    public async Task DeleteAttachmentAsync(uint requestingUserId, uint lessonId, uint attachmentId, CancellationToken ct)
    {
        await EnsureCanManageLessonAsync(requestingUserId, lessonId, ct);
        int deleted = await ctx.Db.LessonAttachments
            .Where(attachment => attachment.Id == attachmentId && attachment.LessonId == lessonId)
            .ExecuteDeleteAsync(ct);
        if (deleted == 0)
            throw ServiceException.NotFound(Resources.ErrorLessonAttachmentNotFound);
    }

    private async Task EnsureCanManageLessonAsync(uint requestingUserId, uint lessonId, CancellationToken ct)
    {
        uint? courseId = await ctx.Db.Lessons.AsNoTracking()
            .Where(lesson => lesson.Id == lessonId)
            .Select(lesson => (uint?)lesson.Section.CourseId)
            .FirstOrDefaultAsync(ct);
        if (!courseId.HasValue)
            throw ServiceException.NotFound(Resources.ErrorLessonNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, requestingUserId, courseId.Value, ct);
    }

    private async Task EnsureCanManageSectionAsync(uint requestingUserId, uint sectionId, CancellationToken ct)
    {
        uint? courseId = await ctx.Db.Sections.AsNoTracking()
            .Where(section => section.Id == sectionId)
            .Select(section => (uint?)section.CourseId)
            .FirstOrDefaultAsync(ct);
        if (!courseId.HasValue)
            throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, requestingUserId, courseId.Value, ct);
    }
}