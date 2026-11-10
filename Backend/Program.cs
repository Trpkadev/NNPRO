using System.Security.Claims;
using System.Text;
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
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase))
{
    for (DirectoryInfo? directory = new(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
    {
        string envFilePath = Path.Combine(directory.FullName, ".env");
        if (!File.Exists(envFilePath))
            continue;

        Env.Load(envFilePath, new LoadOptions(clobberExistingVars: false));
        break;
    }
}

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"[Startup] ASP.NET Core environment: {builder.Environment.EnvironmentName}");

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException(Resources.ErrorDefaultConnectionStringMissing);

builder.Services.Configure<DatabaseLimitsOptions>(builder.Configuration.GetSection(DatabaseLimitsOptions.SectionName));
builder.Services.Configure<RouteOptions>(options => options.ConstraintMap.Add("uint", typeof(UintRouteConstraint)));
builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var serverVersion = await ServerVersion.AutoDetectAsync(connectionString);
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseMySql(connectionString, serverVersion));
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "Elearning_";
});

builder.Services.AddControllers(options => { options.Filters.Add<ApiResponseWrapperFilter>(); })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(options => {
        options.InvalidModelStateResponseFactory = context => {
            var problemDetails = new ValidationProblemDetails(context.ModelState) {
                Title = Resources.ErrorBadRequestTitle,
                Detail = Resources.ErrorBadRequestDescription,
                Status = StatusCodes.Status400BadRequest,
                Instance = context.HttpContext.Request.Path,
                Extensions = { ["traceId"] = context.HttpContext.TraceIdentifier }
            };
            return new BadRequestObjectResult(problemDetails);
        };
    });

builder.Services.AddResponseCaching();

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy("Frontend",
        policy => {
            if (allowedOrigins.Length > 0)
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            else if (builder.Environment.IsDevelopment())
                policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
        })
);

builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("PublicGetPolicy",
        opt => {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
        });
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? throw new InvalidOperationException(Resources.ErrorJwtConfigurationMissing);

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Check the Redis token blacklist after validating the JWT.
    options.Events = new JwtBearerEvents {
        OnTokenValidated = async context => {
            var blacklistService = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
            string? jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);

            if (!string.IsNullOrEmpty(jti) && await blacklistService.IsBlacklistedAsync(jti, context.HttpContext.RequestAborted))
                context.Fail(Resources.ErrorAccessTokenRevoked);
        }
    };
});
builder.Services.AddAuthorization();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<PaginationNeededExceptionHandler>();
builder.Services.AddExceptionHandler<InvalidFilterExceptionHandler>();
builder.Services.AddExceptionHandler<InvalidSortExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi(options => {
    options.AddDocumentTransformer((document, _, _) => {
        document.Info.Title = Resources.OpenApiTitle;
        document.Info.Version = "v1";
        document.Info.Description = Resources.OpenApiDescription;

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = Resources.OpenApiBearerDescription
        };

        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, _) => {
        if (operation.Parameters is not null)
            foreach (var param in operation.Parameters)
                if (param.Name != null && param.Name.Equals("filters", StringComparison.OrdinalIgnoreCase) && param is OpenApiParameter concreteParam)
                    concreteParam.Schema = new OpenApiSchema {
                        Type = JsonSchemaType.String,
                        Examples = [JsonValue.Create("{\"title\":\"C#\"}")]
                    };

        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        bool hasAuthorize = metadata.OfType<IAuthorizeData>().Any();
        bool hasAllowAnonymous = metadata.OfType<IAllowAnonymous>().Any();

        operation.Security ??= [];
        operation.Security.Clear();

        if (!hasAuthorize || hasAllowAnonymous)
            return Task.CompletedTask;

        var schemeRef = new OpenApiSecuritySchemeReference("Bearer");

        operation.Security.Add(new OpenApiSecurityRequirement { [schemeRef] = [] });

        return Task.CompletedTask;
    });
});

builder.Services.AddHealthChecks();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();

builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(@"/home/app/dp_keys"));
var app = builder.Build();

app.UseExceptionHandler();

// Forwarded headers must be applied before rate limiting to resolve the client IP.
app.UseForwardedHeaders();

app.UseHttpsRedirection();

// Apply CORS before rate limiting and authentication so rejected requests retain CORS headers.
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();

    await using var context = await factory.CreateDbContextAsync();
    await context.Database.MigrateAsync();

    await DbInitializer.SeedCoursesAsync(factory);

    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();