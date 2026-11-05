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
    /// Vrátí detail konkrétního kurzu podle jeho ID.
    /// </summary>
    /// <param name="id">Unikátní identifikátor kurzu.</param>
    /// <param name="cancellationToken">Token pro zrušení požadavku.</param>
    [HttpGet("{id:int}")]
    [EndpointSummary("Detail kurzu")]
    [EndpointDescription("Vrátí detail kurzu včetně názvu, popisu a případného přístupového klíče.")]
    [ProducesResponseType<Wrapper<CourseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDto>> GetCourseById(uint id, CancellationToken cancellationToken)
    {
        var serviceResult = await courseService.GetByIdAsync(id, cancellationToken);

        if (serviceResult is null)
            return NotFound();

        return Ok(serviceResult);
    }

    /// <summary>
    /// Vrátí stránkovaný seznam kurzů ve zkrácené podobě.
    /// </summary>
    /// <param name="query">Parametry stránkování, filtrování a řazení.</param>
    /// <param name="cancellationToken">Token pro zrušení požadavku.</param>
    [HttpGet("thumbnails")]
    [EndpointSummary("Seznam náhledů kurzů")]
    [EndpointDescription("Vrátí kolekci kurzů v odlehčené podobě pro katalog nebo karty kurzů. Podporuje stránkování, textové hledání, omezení délky popisu a řazení.")]
    [ProducesResponseType<Wrapper<CourseThumbnailDto[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CourseThumbnailDto[]>> GetCourseThumbnails([FromQuery] CourseThumbnailQuery query, CancellationToken cancellationToken)
    {
        var sortableProperties = typeof(Course).GetProperties().Select(property => property.Name);
        if (!sortableProperties.Contains(query.SortBy, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new ProblemDetails {
                Title = Resources.ErrorTitleInvalidSortBy,
                Detail = string.Format(Resources.ErrorDescriptionInvalidSortBy, query.SortBy, Resources.Course),
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await courseService.GetCourseThumbnailsAsync(query, cancellationToken);

        return Ok(result);
    }
}