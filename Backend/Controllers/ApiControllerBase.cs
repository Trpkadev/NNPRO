using Backend.Exceptions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend.Controllers;

[ApiController]
[Produces("application/json")]
[EnableRateLimiting("PublicGetPolicy")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public class ApiControllerBase : ControllerBase {
    protected OkObjectResult Ok<T>(T data, Metadata metadata) => base.Ok(new Wrapper<T>(data, metadata));

    protected OkObjectResult OkPaged<T>(T[] items, RequestQuery query)
    {
        if (query.Page is null || query.PageSize is null)
            throw new PaginationNeededException();

        int page = query.Page.Value;
        int pageSize = query.PageSize.Value;
        long offset = (long)(page - 1) * pageSize;
        int skip = (int)Math.Min(offset, items.Length);
        var pageItems = items.Skip(skip).Take(pageSize).ToArray();
        var pagination = new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = items.Length };

        return Ok(pageItems, new Metadata(pagination, query));
    }
}