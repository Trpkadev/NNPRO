using Backend.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public class ApiControllerBase : ControllerBase {
    protected OkObjectResult Ok<T>(T data, Metadata metadata) => base.Ok(new Wrapper<T>(data, metadata));
}