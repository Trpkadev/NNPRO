using Backend.Models.DTOs.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Extensions;

[AttributeUsage(AttributeTargets.Method)]
public sealed class RequiresPagingAttribute : ActionFilterAttribute {
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var query = context.ActionArguments.Values.OfType<RequestQuery>().FirstOrDefault() ?? throw new InvalidOperationException(string.Format(Resources.ErrorRequiresPagingActionParameterMissing, nameof(RequiresPagingAttribute), nameof(RequestQuery)));

        if (query is { Page: not null, PageSize: not null })
            return;

        if (query.Page == null)
            context.ModelState.AddModelError(nameof(RequestQuery.Page), Resources.ErrorPagingPageRequired);
        if (query.PageSize == null)
            context.ModelState.AddModelError(nameof(RequestQuery.PageSize), Resources.ErrorPagingPageSizeRequired);

        context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState) {
            Title = Resources.ErrorBadRequestTitle,
            Detail = Resources.ErrorPaginationRequired,
            Status = StatusCodes.Status400BadRequest,
            Instance = context.HttpContext.Request.Path,
            Extensions = { ["traceId"] = context.HttpContext.TraceIdentifier }
        });
    }
}