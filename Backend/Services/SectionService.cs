using Backend.Exceptions;
using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class SectionService(ServiceContext<SectionService> ctx) : ISectionService {
    public async Task<SectionResponseDto> GetByIdAsync(uint sectionId, CancellationToken ct)
    {
        var section = await ctx.Db.Sections.AsNoTracking()
            .Where(candidate => candidate.Id == sectionId)
            .Select(candidate => new {
                candidate.Id,
                candidate.Title,
                candidate.Order,
                candidate.CourseId
            })
            .FirstOrDefaultAsync(ct);
        if (section is null)
            throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, section.CourseId, ctx.AuthUserId, ct);
        return new SectionResponseDto(section.Id, section.Title, section.Order, section.CourseId);
    }

    public async Task<(LessonResponseWrapperDto Lessons, Metadata Metadata)> GetLessonsAsync(uint sectionId, RequestQuery request, CancellationToken ct)
    {
        uint courseId = await ctx.Db.Sections.AsNoTracking()
                            .Where(s => s.Id == sectionId)
                            .Select(s => (uint?)s.CourseId)
                            .FirstOrDefaultAsync(ct)
                        ?? throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(ctx.Db, courseId, ctx.AuthUserId, ct);

        var (lessons, metadata) = await ctx.Db.Lessons.AsNoTracking()
            .Where(lesson => lesson.SectionId == sectionId)
            .OrderBy(lesson => lesson.Order)
            .ThenBy(lesson => lesson.Id)
            .Select(lesson => new LessonResponseDto(lesson.Id, lesson.Title, lesson.Content, lesson.VideoUrl, lesson.Order, lesson.SectionId))
            .ToPagedResultAsync(request, ctx.MaxUnpagedItems, ct);
        uint completedLessons = (uint)await ctx.Db.StudentLessonProgresses.AsNoTracking()
            .Where(lesson => lesson.StudentId == ctx.AuthUserId && lesson.Lesson.SectionId == sectionId)
            .CountAsync(ct);

        return (new LessonResponseWrapperDto(lessons, (uint)lessons.Length, completedLessons), new Metadata(metadata, request));
    }

    public async Task<(QuizResponseDto[] Quizzes, Metadata Metadata)> GetQuizzesAsync(uint sectionId, RequestQuery request, CancellationToken ct)
    {
        uint courseId = await ctx.Db.Sections.AsNoTracking()
                            .Where(s => s.Id == sectionId)
                            .Select(s => (uint?)s.CourseId)
                            .FirstOrDefaultAsync(ct)
                        ?? throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(ctx.Db, courseId, ctx.AuthUserId, ct);

        var (quizzes, metadata) = await ctx.Db.Quizzes.AsNoTracking()
            .Where(quiz => quiz.SectionId == sectionId)
            .OrderBy(quiz => quiz.Id)
            .Select(quiz => new QuizResponseDto(quiz.Id, quiz.Title, quiz.MaxAttempts, quiz.TimeLimitSeconds, quiz.PassingThresholdPercent, quiz.SectionId))
            .ToPagedResultAsync(request, ctx.MaxUnpagedItems, ct);

        return (quizzes, new Metadata(metadata, request));
    }

    public async Task<uint> CreateAsync(CreateSectionRequestDto request, CancellationToken ct)
    {
        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, request.CourseId, ctx.AuthUserId, ct);
        Section section = new() {
            Title = request.Title,
            Order = request.Order,
            CourseId = request.CourseId
        };
        ctx.Db.Sections.Add(section);
        await ctx.Db.SaveChangesAsync(ct);
        return section.Id;
    }

    public async Task UpdateAsync(uint sectionId, CreateSectionRequestDto request, CancellationToken ct)
    {
        var section = await ctx.Db.Sections.FirstOrDefaultAsync(candidate => candidate.Id == sectionId, ct) ?? throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, section.CourseId, ctx.AuthUserId, ct);
        if (section.CourseId != request.CourseId)
            await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, section.CourseId, ctx.AuthUserId, ct);

        section.Title = request.Title;
        section.Order = request.Order;
        section.CourseId = request.CourseId;
        await ctx.Db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(uint sectionId, CancellationToken ct)
    {
        var section = await ctx.Db.Sections.AsNoTracking()
                          .Where(candidate => candidate.Id == sectionId)
                          .Select(candidate => new { candidate.Id, candidate.CourseId })
                          .FirstOrDefaultAsync(ct)
                      ?? throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(ctx.Db, section.CourseId, ctx.AuthUserId, ct);
        await ctx.Db.Sections.Where(candidate => candidate.Id == sectionId).ExecuteDeleteAsync(ct);
    }
}