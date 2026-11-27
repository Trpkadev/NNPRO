using Backend.Extensions;
using DotNetEnv;
using Scalar.AspNetCore;

if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase))
    Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppConfiguration(builder.Configuration)
    .AddApiControllersAndErrorHandling()
    .AddAppCorsAndRateLimiting()
    .AddJwtAuthentication()
    .AddOpenApiDocumentation()
    .AddApplicationServices()
    .AddPersistence(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

// Forwarded headers must be applied before rate limiting to resolve the client IP.
app.UseForwardedHeaders();

// Apply CORS before rate limiting and authentication so rejected requests retain CORS headers.
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseResponseCompression();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeDevelopmentDatabaseAsync(app.Lifetime.ApplicationStopping);
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
}

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();