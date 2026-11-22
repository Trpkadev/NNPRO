using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

namespace Backend.Extensions;

public static class OpenApiExtensions {
    private const string BearerSecuritySchemeName = "Bearer";
    private const string RefreshCookieSchemeName = "RefreshCookie";

    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options => {
            options.AddDocumentTransformer((document, _, _) => {
                document.Info.Title = Resources.OpenApiTitle;
                document.Info.Version = "v1";
                document.Info.Description = Resources.OpenApiDescription;

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[BearerSecuritySchemeName] = new OpenApiSecurityScheme {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = Resources.OpenApiBearerDescription
                };
                document.Components.SecuritySchemes[RefreshCookieSchemeName] = new OpenApiSecurityScheme {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Cookie,
                    Name = "refreshToken",
                    Description = "HttpOnly refresh-token cookie set by login/register and rotated by the refresh endpoint."
                };

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, _) => {
                var filterParam = operation.Parameters?.OfType<OpenApiParameter>()
                    .FirstOrDefault(p => p.Name?.Equals("filters", StringComparison.OrdinalIgnoreCase) == true);

                filterParam?.Schema = new OpenApiSchema {
                    Type = JsonSchemaType.String,
                    Examples = [JsonValue.Create("{\"title\":\"C#\"}")]
                };

                operation.Security ??= [];
                operation.Security.Clear();

                string relativePath = context.Description.RelativePath?.TrimStart('/') ?? string.Empty;

                if (relativePath.StartsWith("api/auth/", StringComparison.OrdinalIgnoreCase))
                {
                    ConfigureAuthEndpoint(operation, context.Document, relativePath[9..].ToLowerInvariant());
                    return Task.CompletedTask;
                }

                var metadata = context.Description.ActionDescriptor.EndpointMetadata;
                bool hasAuthorize = metadata.OfType<IAuthorizeData>().Any();
                bool hasAllowAnonymous = metadata.OfType<IAllowAnonymous>().Any();

                if (hasAuthorize && !hasAllowAnonymous)
                    operation.Security.Add(new OpenApiSecurityRequirement {
                        [new OpenApiSecuritySchemeReference(BearerSecuritySchemeName, context.Document)] = []
                    });

                return Task.CompletedTask;
            });
        });

        return services;
    }

    private static void ConfigureAuthEndpoint(OpenApiOperation operation, OpenApiDocument? document, string action)
    {
        var cookieHeader = action switch {
            "refresh" => ("200", "Rotated HttpOnly refreshToken cookie."),
            "login" or "register" => ("200", "HttpOnly refreshToken cookie for the new session."),
            "logout" => ("204", "Expires the HttpOnly refreshToken cookie."),
            _ => default((string StatusCode, string Description)?)
        };

        if (cookieHeader.HasValue)
            AddSetCookieResponseHeader(operation, cookieHeader.Value.StatusCode, cookieHeader.Value.Description);

        switch (action)
        {
            case "refresh":
                operation.Security!.Add(new OpenApiSecurityRequirement {
                    [new OpenApiSecuritySchemeReference(RefreshCookieSchemeName, document)] = []
                });
                operation.Parameters ??= [];
                operation.Parameters.Add(new OpenApiParameter {
                    Name = "Origin",
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = "Must match an allowed frontend origin or the API origin.",
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String }
                });
                break;

            case "login":
                operation.Security!.Add(new OpenApiSecurityRequirement());
                operation.Security.Add(new OpenApiSecurityRequirement {
                    [new OpenApiSecuritySchemeReference(BearerSecuritySchemeName, document)] = []
                });
                break;

            case "logout":
                operation.Security!.Add(new OpenApiSecurityRequirement());
                operation.Security.Add(new OpenApiSecurityRequirement {
                    [new OpenApiSecuritySchemeReference(BearerSecuritySchemeName, document)] = []
                });
                operation.Security.Add(new OpenApiSecurityRequirement {
                    [new OpenApiSecuritySchemeReference(RefreshCookieSchemeName, document)] = []
                });
                operation.Parameters ??= [];
                if (!operation.Parameters.OfType<OpenApiParameter>().Any(parameter =>
                        parameter.In == ParameterLocation.Header && parameter.Name?.Equals("Origin", StringComparison.OrdinalIgnoreCase) == true))
                    operation.Parameters.Add(new OpenApiParameter {
                        Name = "Origin",
                        In = ParameterLocation.Header,
                        Required = false,
                        Description = "Must match an allowed origin when the refreshToken cookie is sent.",
                        Schema = new OpenApiSchema { Type = JsonSchemaType.String }
                    });
                break;
        }
    }

    private static void AddSetCookieResponseHeader(OpenApiOperation operation, string statusCode, string description)
    {
        if (operation.Responses is null || !operation.Responses.TryGetValue(statusCode, out var response) || response is not OpenApiResponse concreteResponse)
            return;

        concreteResponse.Headers ??= new Dictionary<string, IOpenApiHeader>();
        concreteResponse.Headers["Set-Cookie"] = new OpenApiHeader {
            Description = description,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        };
    }
}