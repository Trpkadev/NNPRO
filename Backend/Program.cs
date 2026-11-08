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
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"[Startup] ASP.NET Core environment: {builder.Environment.EnvironmentName}");

// ==========================================
// CONFIGURATION & OPTIONS
// ==========================================
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                          ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.Configure<DatabaseLimitsOptions>(builder.Configuration.GetSection(DatabaseLimitsOptions.SectionName));
builder.Services.Configure<RouteOptions>(options => options.ConstraintMap.Add("uint", typeof(UintRouteConstraint)));
builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ==========================================
// DATABASE
// ==========================================
var serverVersion = await ServerVersion.AutoDetectAsync(connectionString);
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseMySql(connectionString, serverVersion));

// ==========================================
// CORE API SERVICES
// ==========================================
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

builder.Services.AddResponseCaching();

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => {
    options.AddPolicy("Frontend",
        policy => {
            if (allowedOrigins.Length > 0)
                policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        });
});

builder.Services.AddRateLimiter(options => {
    options.AddFixedWindowLimiter("PublicGetPolicy",
        opt => {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
        });
});

// ==========================================
// AUTH
// ==========================================
builder.Services.AddAuthorization();

// ==========================================
// EXCEPTION HANDLING
// ==========================================
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<PaginationNeededExceptionHandler>();
builder.Services.AddExceptionHandler<InvalidFilterExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ==========================================
// OPENAPI & HEALTH CHECKS
// ==========================================
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
            {
                concreteParam.Schema = new OpenApiSchema {
                    Type = JsonSchemaType.String,
                    Examples = [JsonValue.Create("{\"title\":\"C#\"}")]
                };
            }
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddHealthChecks();

// ==========================================
// SERVICES
// ==========================================
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();


// ==========================================
// BUILD APPLICATION
// ==========================================
var app = builder.Build();

// ==========================================
// HTTP REQUEST PIPELINE
// ==========================================

app.UseExceptionHandler();

// IP translation (RateLimiter needs client IP)
app.UseForwardedHeaders();

app.UseHttpsRedirection();

// CORS (always before RateLimiter & Auth, client would get CORS errors 401/429)
app.UseCors("Frontend");

app.UseRateLimiter();
app.UseAuthorization();
app.UseResponseCaching();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await DbInitializer.SeedCoursesAsync(factory);

    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();