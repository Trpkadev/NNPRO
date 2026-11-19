using Backend.Exceptions;
using Backend.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend.Controllers;

[ApiController]
[Produces("application/json")]
[EnableRateLimiting("PublicGetPolicy")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public class ApiControllerBase : ControllerBase {
    protected uint AuthenticatedUserId => uint.TryParse(User.FindFirst("sub")?.Value, out uint userId) ? userId : throw ServiceException.Unauthorized(Resources.ErrorAuthenticatedUserIdentityInvalid);

    protected OkObjectResult Ok<T>(T data, Metadata metadata) => base.Ok(new Wrapper<T>(data, metadata));
}