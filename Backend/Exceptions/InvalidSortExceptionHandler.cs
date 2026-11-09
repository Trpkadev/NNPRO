using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Exceptions;

public class InvalidSortExceptionHandler : IExceptionHandler {
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not InvalidSortException typedException)
            return false;

        string title = string.Format(Resources.ExceptionTitleInvalidSort, typedException.SortBy);
        var problemDetails = new ProblemDetails {
            Status = StatusCodes.Status400BadRequest,
            Title = title,
            Detail = title
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}