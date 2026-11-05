using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using Backend.Models.DTOs.Response;

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
            SetServerMetadata(existingWrapper, stopwatch.ElapsedMilliseconds);
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

    private static void SetServerMetadata(IApiResponseWrapper response, long executionTimeMs)
    {
        response.Metadata ??= new Metadata();
        response.Metadata.ServerInfo ??= new ServerMetadata();
        response.Metadata.ServerInfo.Timestamp = DateTime.UtcNow;
        response.Metadata.ServerInfo.ExecutionTimeMs = executionTimeMs;
    }

    private static Metadata CreateMetadata(long executionTimeMs) => new() { ServerInfo = new ServerMetadata { ExecutionTimeMs = executionTimeMs } };
}