using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Exceptions;

public class PaginationNeededExceptionHandler : IExceptionHandler {
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not PaginationNeededException)
            return false;

        var problemDetails = new ProblemDetails {
            Status = StatusCodes.Status400BadRequest,
            Title = Resources.ExceptionTitlePaginationNeeded,
            Detail = Resources.ExceptionTitlePaginationNeeded
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}