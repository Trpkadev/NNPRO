using System.Diagnostics;
using Backend.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Middleware;

public class ApiResponseWrapperFilter : IAsyncActionFilter {
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var executedContext = await next();
        stopwatch.Stop();

        if (executedContext.Result is not ObjectResult { Value: not null } objectResult || objectResult.StatusCode is < 200 or >= 300)
            return;

        if (objectResult.Value is IApiResponseWrapper existingWrapper)
        {
            existingWrapper.Metadata ??= new Metadata();
            existingWrapper.Metadata.ServerInfo = new ServerMetadata { Timestamp = DateTime.UtcNow, ExecutionTimeMs = stopwatch.ElapsedMilliseconds };
            return;
        }

        var valueType = objectResult.Value.GetType();
        var wrapperType = typeof(Wrapper<>).MakeGenericType(valueType);
        var metadata = CreateMetadata(stopwatch.ElapsedMilliseconds);
        object? wrappedValue = Activator.CreateInstance(wrapperType, objectResult.Value, metadata);
        if (wrappedValue == null)
            return;

        executedContext.Result = new ObjectResult(wrappedValue) {
            ContentTypes = objectResult.ContentTypes,
            DeclaredType = objectResult.DeclaredType,
            StatusCode = objectResult.StatusCode
        };
    }

    private static Metadata CreateMetadata(long executionTimeMs) => new() { ServerInfo = new ServerMetadata { ExecutionTimeMs = executionTimeMs } };
}