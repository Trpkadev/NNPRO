using Backend.Extensions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[Route("api/courses")]
[Tags("Courses")]
public class CourseController(ICourseService courseService) : ApiControllerBase {
    [HttpGet("{id:uint}")]
    [Authorize]
    [EndpointSummary("Get course detail")]
    [EndpointDescription("Returns the full details of a course including its title and description.")]
    [ProducesResponseType<Wrapper<CourseResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseResponseDto>> GetCourseById(uint id, CancellationToken cancellationToken) => Ok(await courseService.GetByIdAsync(id, AuthenticatedUserId, cancellationToken));

    [HttpGet]
    [RequiresPaging]
    [ResponseCache(Duration = 60, VaryByQueryKeys = ["*"])]
    [Authorize]
    [EndpointSummary("Get all courses")]
    [EndpointDescription("Returns a list of courses including their details and assigned instructors.")]
    [ProducesResponseType<Wrapper<CourseResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CourseResponseDto[]>> GetCourses([FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        var (courses, metadata) = await courseService.GetAsync(requestQuery, cancellationToken);

        return Ok(courses, metadata);
    }

    [HttpGet("{courseId:uint}/instructors")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get course instructors")]
    [ProducesResponseType<Wrapper<CourseInstructorResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseInstructorResponseDto[]>> GetCourseInstructors(uint courseId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        var (instructors, metadata) = await courseService.GetInstructorsAsync(courseId, requestQuery, cancellationToken);
        return Ok(instructors, metadata);
    }

    [HttpGet("{courseId:uint}/sections")]
    [Authorize]
    [RequiresPaging]
    [EndpointSummary("Get course sections")]
    [ProducesResponseType<Wrapper<SectionResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SectionResponseDto[]>> GetCourseSections(uint courseId, [FromQuery] RequestQuery requestQuery, CancellationToken cancellationToken)
    {
        var (sections, metadata) = await courseService.GetSectionsAsync(courseId, AuthenticatedUserId, requestQuery, cancellationToken);
        return Ok(sections, metadata);
    }

    [HttpPost]
    [EndpointSummary("Create a course")]
    [Authorize(Roles = "Admin, Instructor")]
    [EndpointDescription("Creates a new course and returns the created course id.")]
    [ProducesResponseType<Wrapper<uint>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<uint>> AddCourse([FromBody] CourseCreateRequestDto courseCreateRequestDto, CancellationToken cancellationToken)
    {
        uint newCourseId = await courseService.CreateAsync(AuthenticatedUserId, courseCreateRequestDto, cancellationToken);
        return CreatedAtAction(nameof(GetCourseById), new { id = newCourseId }, newCourseId);
    }

    [HttpDelete("{id:uint}")]
    [EndpointSummary("Delete a course")]
    [Authorize(Roles = "Admin, Instructor")]
    [EndpointDescription("Removes a course from the system by its ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCourse(uint id, CancellationToken cancellationToken)
    {
        await courseService.RemoveAsync(AuthenticatedUserId, id, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:uint}")]
    [Authorize(Roles = "Admin, Instructor")]
    [EndpointSummary("Update a course")]
    [EndpointDescription("Fully updates an existing course with new details.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourse(uint id, [FromBody] CourseUpdateRequestDto courseUpdateRequestDto, CancellationToken cancellationToken)
    {
        await courseService.UpdateAsync(AuthenticatedUserId, id, courseUpdateRequestDto, cancellationToken);
        return NoContent();
    }

    [HttpGet("thumbnails")]
    [RequiresPaging]
    [ResponseCache(Duration = 60, VaryByQueryKeys = ["*"])]
    [Authorize]
    [EndpointSummary("Get course thumbnails")]
    [EndpointDescription("Returns a lightweight collection of courses for catalog or card display. Supports pagination, text search, description trimming, and sorting.")]
    [ProducesResponseType<Wrapper<CourseThumbnailResponseDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CourseThumbnailResponseDto[]>> GetCourseThumbnails([FromQuery] CourseThumbnailRequestDto requestDto, CancellationToken cancellationToken)
    {
        var (thumbnails, metadata) = await courseService.GetCourseThumbnailsAsync(requestDto, cancellationToken);

        return Ok(thumbnails, metadata);
    }
}