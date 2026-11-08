using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Exceptions;

public class InvalidFilterExceptionHandler : IExceptionHandler {
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not InvalidFilterException typedException)
            return false;

        var problemDetails = new ProblemDetails {
            Status = StatusCodes.Status400BadRequest,
            Title = string.Format(Resources.ExceptionTitleInvalidFilter, typedException.FilterParam),
            Detail = string.Format(Resources.ExceptionTitleInvalidFilter, typedException.FilterParam)
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}