using System.Text.Json.Serialization;
using Backend;
using Backend.Data;
using Backend.Extensions;
using Backend.Middleware;
using Backend.Services;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
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
});
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();

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
var app = builder.Build();

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