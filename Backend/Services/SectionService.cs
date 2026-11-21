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

public class SectionService(AppDbContext dbContext, IOptions<DatabaseLimitsOptions> dbLimitsOptions) : ISectionService {
    public async Task<SectionResponseDto> GetByIdAsync(uint requestingUserId, uint sectionId, CancellationToken ct)
    {
        var section = await dbContext.Sections.AsNoTracking()
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

        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, section.CourseId, ct);
        return new SectionResponseDto(section.Id, section.Title, section.Order, section.CourseId);
    }

    public async Task<(LessonResponseWrapperDto Lessons, Metadata Metadata)> GetLessonsAsync(uint sectionId, uint userId, RequestQuery request, CancellationToken ct)
    {
        uint courseId = await dbContext.Sections.AsNoTracking()
                            .Where(s => s.Id == sectionId)
                            .Select(s => (uint?)s.CourseId)
                            .FirstOrDefaultAsync(ct)
                        ?? throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(dbContext, courseId, userId, ct);

        var (lessons, metadata) = await dbContext.Lessons.AsNoTracking()
            .Where(lesson => lesson.SectionId == sectionId)
            .OrderBy(lesson => lesson.Order)
            .ThenBy(lesson => lesson.Id)
            .Select(lesson => new LessonResponseDto(lesson.Id, lesson.Title, lesson.Content, lesson.VideoUrl, lesson.Order, lesson.SectionId))
            .ToPagedResultAsync(request, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);
        uint completedLessons = (uint)await dbContext.StudentLessonProgresses.AsNoTracking()
            .Where(lesson => lesson.StudentId == userId && lesson.Lesson.SectionId == sectionId)
            .CountAsync(ct);

        return (new LessonResponseWrapperDto(lessons, (uint)lessons.Length, completedLessons), new Metadata(metadata, request));
    }

    public async Task<(QuizResponseDto[] Quizzes, Metadata Metadata)> GetQuizzesAsync(uint sectionId, uint userId, RequestQuery request, CancellationToken ct)
    {
        uint courseId = await dbContext.Sections.AsNoTracking()
                            .Where(s => s.Id == sectionId)
                            .Select(s => (uint?)s.CourseId)
                            .FirstOrDefaultAsync(ct)
                        ?? throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanAccessCourseAsync(dbContext, courseId, userId, ct);

        var (quizzes, metadata) = await dbContext.Quizzes.AsNoTracking()
            .Where(quiz => quiz.SectionId == sectionId)
            .OrderBy(quiz => quiz.Id)
            .Select(quiz => new QuizResponseDto(quiz.Id, quiz.Title, quiz.MaxAttempts, quiz.TimeLimitSeconds, quiz.PassingThresholdPercent, quiz.SectionId))
            .ToPagedResultAsync(request, dbLimitsOptions.Value.MaxItemsLoadedWithoutPagination, ct);

        return (quizzes, new Metadata(metadata, request));
    }

    public async Task<uint> CreateAsync(uint requestingUserId, CreateSectionRequestDto request, CancellationToken ct)
    {
        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, request.CourseId, ct);
        Section section = new() {
            Title = request.Title,
            Order = request.Order,
            CourseId = request.CourseId
        };
        dbContext.Sections.Add(section);
        await dbContext.SaveChangesAsync(ct);
        return section.Id;
    }

    public async Task UpdateAsync(uint requestingUserId, uint sectionId, CreateSectionRequestDto request, CancellationToken ct)
    {
        var section = await dbContext.Sections.FirstOrDefaultAsync(candidate => candidate.Id == sectionId, ct);
        if (section is null)
            throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, section.CourseId, ct);
        if (section.CourseId != request.CourseId)
            await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, request.CourseId, ct);

        section.Title = request.Title;
        section.Order = request.Order;
        section.CourseId = request.CourseId;
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(uint requestingUserId, uint sectionId, CancellationToken ct)
    {
        var section = await dbContext.Sections.AsNoTracking()
            .Where(candidate => candidate.Id == sectionId)
            .Select(candidate => new { candidate.Id, candidate.CourseId })
            .FirstOrDefaultAsync(ct);
        if (section is null)
            throw ServiceException.NotFound(Resources.ErrorSectionNotFound);

        await CourseAuthorization.EnsureCanManageCourseAsync(dbContext, requestingUserId, section.CourseId, ct);
        await dbContext.Sections.Where(candidate => candidate.Id == sectionId).ExecuteDeleteAsync(ct);
    }
}