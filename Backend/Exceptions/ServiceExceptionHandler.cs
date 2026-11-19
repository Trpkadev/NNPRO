using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Exceptions;

public sealed class ServiceExceptionHandler : IExceptionHandler {
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ServiceException serviceException)
            return false;

        var problemDetails = new ProblemDetails {
            Status = serviceException.StatusCode,
            Title = serviceException.Message,
            Detail = serviceException.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = serviceException.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}