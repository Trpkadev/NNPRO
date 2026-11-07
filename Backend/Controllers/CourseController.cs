using Backend;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
public class CourseController(ICourseService courseService) : ApiControllerBase {
    /// <summary>
    /// Returns the detail of a specific course by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the course.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    [HttpGet("{id:uint}")]
    [EndpointSummary("Get course detail")]
    [EndpointDescription("Returns the full details of a course including title, description, and optional access key.")]
    [ProducesResponseType<Wrapper<CourseResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseResponseDto>> GetCourseById(uint id, CancellationToken cancellationToken)
    {
        var serviceResult = await courseService.GetByIdAsync(id, cancellationToken);

        if (serviceResult is null)
            return NotFound();

        return Ok(serviceResult);
    }

    /// <summary>
    /// Returns a list of courses based on filter criteria.
    /// </summary>
    /// <param name="courseRequestDto">Filtering and pagination options.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    [HttpGet]
    [EndpointSummary("Get all courses")]
    [EndpointDescription("Returns a list of courses including their details and assigned instructors.")]
    [ProducesResponseType<Wrapper<IEnumerable<CourseResponseDto>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CourseResponseDto>>> GetCourses([FromQuery] CourseRequestDto courseRequestDto, CancellationToken cancellationToken)
    {
        List<User> users = [new() { Email = "dummy@dummy.dummy", Id = 1, PasswordHash = "dummy", Role = UserRole.Instructor, Username = "dummy" }];
        List<User> users2 = [new() { Email = "admindummy@dummy.dummy", Id = 1, PasswordHash = "admindummy", Role = UserRole.Admin, Username = "admindummy" }];
        Course[] courses = [
            new() { Description = "dummy1", Title = "dummy1", AccessKey = "dummy", Id = 1, Instructors = users },
            new() { Description = "dummy2", Title = "dummy3", AccessKey = null, Id = 2, Instructors = users2 }
        ];
        return Ok(courses);
    }

    /// <summary>
    /// Creates a new course.
    /// </summary>
    /// <param name="coursePostRequestDto">Data required to create a new course.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    [HttpPost]
    [EndpointSummary("Create a course")]
    [EndpointDescription("Creates a new course and returns the created course details.")]
    [ProducesResponseType<Wrapper<CourseResponseDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CourseResponseDto>> AddCourse([FromBody] CoursePostRequestDto coursePostRequestDto, CancellationToken cancellationToken)
    {
        Course course = new() { Description = coursePostRequestDto.Description, Title = coursePostRequestDto.Title, AccessKey = coursePostRequestDto.AccessKey, Instructors = [] };
        return Created();
    }

    /// <summary>
    /// Deletes an existing course by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the course to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    [HttpDelete("{id:uint}")]
    [EndpointSummary("Delete a course")]
    [EndpointDescription("Removes a course from the system by its ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCourse(uint id, CancellationToken cancellationToken) => NoContent();

    /// <summary>
    /// Updates an existing course by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the course to update.</param>
    /// <param name="courseUpdateRequestDto">Updated course data.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    [HttpPut("{id:uint}")]
    [EndpointSummary("Update a course")]
    [EndpointDescription("Fully updates an existing course with new details.")]
    [ProducesResponseType<Wrapper<CourseResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseResponseDto>> UpdateCourse(uint id, [FromBody] CourseUpdateRequestDto courseUpdateRequestDto, CancellationToken cancellationToken) => Ok();

    /// <summary>
    /// Returns a paginated list of course thumbnails.
    /// </summary>
    /// <param name="query">Pagination, filtering, and sorting parameters.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    [HttpGet("thumbnails")]
    [EndpointSummary("Get course thumbnails")]
    [EndpointDescription("Returns a lightweight collection of courses for catalog or card display. Supports pagination, text search, description trimming, and sorting.")]
    [ProducesResponseType<Wrapper<CourseThumbnailDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CourseThumbnailDto[]>> GetCourseThumbnails([FromQuery] CourseThumbnailQuery query, CancellationToken cancellationToken)
    {
        var sortableProperties = typeof(Course).GetProperties().Select(property => property.Name);
        if (!sortableProperties.Contains(query.SortBy, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new ProblemDetails {
                Title = Resources.ErrorTitleInvalidSortBy,
                Detail = string.Format(Resources.ErrorDescriptionInvalidSortBy, query.SortBy, Resources.Course),
                Status = StatusCodes.Status400BadRequest
            });

        var result = await courseService.GetCourseThumbnailsAsync(query, cancellationToken);

        return Ok(result);
    }
}