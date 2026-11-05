using Backend.Models.DTOs;
using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/lessons")]
[Tags("Lessons")]
public class LessonController(ILessonService lessonService) : ApiControllerBase {
    /// <summary>
    /// Získá detail konkrétní lekce podle jejího ID.
    /// </summary>
    /// <param name="id">Unikátní identifikátor lekce</param>
    /// <param name="cancellationToken">Storno token operace</param>
    [HttpGet("{id:int}")]
    [EndpointSummary("Detail lekce")]
    [EndpointDescription("Vrátí kompletí informace o konkrétní lekci včetně textového obsahu, odkazů na videa a příloh.")]
    [ProducesResponseType<Wrapper<LessonDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Wrapper<LessonDto>>> GetLessonById(uint id, CancellationToken cancellationToken)
    {
        var serviceResult = await lessonService.GetByIdAsync(id, cancellationToken);

        if (serviceResult is null)
            return NotFound();

        return Ok(serviceResult);
    }
}