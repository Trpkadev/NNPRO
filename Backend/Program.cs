using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Backend;
using Backend.Config;
using Backend.Data;
using Backend.Exceptions;
using Backend.Extensions;
using Backend.Middleware;
using Backend.Services;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"[Startup] ASP.NET Core environment: {builder.Environment.EnvironmentName}");

// Configuration
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

// Services
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers(options => { options.Filters.Add<ApiResponseWrapperFilter>(); })
    .ConfigureApiBehaviorOptions(options => {
        options.InvalidModelStateResponseFactory = context => {
            var problemDetails = new ValidationProblemDetails(context.ModelState) {
                Title = Resources.ErrorTitleBadRequest,
                Detail = Resources.ErrorDescriptionBadRequest,
                Status = StatusCodes.Status400BadRequest,
                Instance = context.HttpContext.Request.Path,
                Extensions = { ["traceId"] = context.HttpContext.TraceIdentifier }
            };
            return new BadRequestObjectResult(problemDetails);
        };
    });
builder.Services.AddOpenApi(options => {
    options.AddDocumentTransformer((document, _, _) => {
        document.Info.Title = "E-learning System API";
        document.Info.Version = "v1";
        document.Info.Description = "API pro správu kurzů, lekcí a kvízů.";
        return Task.CompletedTask;
    });
    options.AddOperationTransformer((operation, _, _) => {
        if (operation.Parameters is null)
            return Task.CompletedTask;

        foreach (var param in operation.Parameters)
        {
            if (param.Name != null && param.Name.Equals("filters", StringComparison.OrdinalIgnoreCase) && param is OpenApiParameter concreteParam)
                concreteParam.Schema = new OpenApiSchema { Type = JsonSchemaType.String };
        }

        return Task.CompletedTask;
    });
});
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();

var dbLimits = builder.Configuration.GetSection(DatabaseLimitsOptions.SectionName).Get<DatabaseLimitsOptions>() ?? new DatabaseLimitsOptions();
builder.Services.AddSingleton(dbLimits);

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options => {
    options.AddPolicy("Frontend",
        policy => {
            if (allowedOrigins.Length > 0)
                policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        });
});
builder.Services.Configure<RouteOptions>(options => options.ConstraintMap.Add("uint", typeof(UintRouteConstraint)));
builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<PaginationNeededExceptionHandler>();
builder.Services.AddExceptionHandler<InvalidFilterExceptionHandler>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

// HTTP request pipeline
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    await DbInitializer.SeedCoursesAsync(app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>());
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();