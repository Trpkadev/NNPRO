using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Backend.Configuration;
using Backend.Data;
using Backend.Exceptions;
using Backend.Middleware;
using Backend.Services;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Extensions;

public static class ServiceCollectionExtensions {
    extension(IServiceCollection services) {
        public IServiceCollection AddAppConfiguration(IConfiguration configuration)
        {
            services.AddValidatedOptions<DatabaseLimitsOptions>();
            services.AddValidatedOptions<Config>();
            services.AddValidatedOptions<JwtOptions>();
            services.AddValidatedOptions<CorsSettings>();

            services.Configure<RouteOptions>(options => options.ConstraintMap.Add("uint", typeof(UintRouteConstraint)));
            services.Configure<ForwardedHeadersOptions>(options => {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownProxies.Clear();
                options.KnownIPNetworks.Clear();
            });
            services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            string? dpKeyDirectory = configuration["DataProtection:KeyDirectory"];
            if (!string.IsNullOrWhiteSpace(dpKeyDirectory))
                services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(dpKeyDirectory));

            return services;
        }

        public async Task AddPersistenceAsync(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException(Resources.ErrorDefaultConnectionStringMissing);

            string redisConnectionString = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException(Resources.ErrorRedisConnectionStringMissing);

            var serverVersion = await ServerVersion.AutoDetectAsync(connectionString);
            services.AddDbContextFactory<AppDbContext>(options => options.UseMySql(connectionString, serverVersion));

            services.AddStackExchangeRedisCache(options => {
                options.Configuration = redisConnectionString;
                options.InstanceName = "Elearning_";
            });

        }

        public IServiceCollection AddApiControllersAndErrorHandling()
        {
            services.AddControllers(options => options.Filters.Add<ApiResponseWrapperFilter>())
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

            services.AddResponseCaching(); // TODO: Add [ResponseCache] to public GET endpoints

            services.AddProblemDetails();
            services.AddExceptionHandler<PaginationNeededExceptionHandler>();
            services.AddExceptionHandler<InvalidFilterExceptionHandler>();
            services.AddExceptionHandler<InvalidSortExceptionHandler>();
            services.AddExceptionHandler<ServiceExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }

        public IServiceCollection AddAppCorsAndRateLimiting()
        {
            services.AddCors();
            services.AddOptions<CorsOptions>()
                .Configure<IOptions<CorsSettings>>((cors, settings) => {
                    string[] allowedOrigins = settings.Value.AllowedOrigins;
                    cors.AddPolicy("Frontend",
                        policy => {
                            if (allowedOrigins.Length > 0)
                                policy.WithOrigins(allowedOrigins)
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials();
                        });
                });

            services.AddRateLimiter(options => {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("PublicGetPolicy",
                    context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        }));
            });

            return services;
        }

        public IServiceCollection AddJwtAuthentication()
        {
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer();

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtOptions>>((options, jwtOpts) => {
                    var jwt = jwtOpts.Value;
                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        RequireExpirationTime = true,
                        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                        NameClaimType = "name",
                        RoleClaimType = "role",
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                    options.MapInboundClaims = false;

                    options.Events = new JwtBearerEvents {
                        OnTokenValidated = async context => {
                            var blacklistService = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
                            string? jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);

                            if (!string.IsNullOrEmpty(jti) && await blacklistService.IsBlacklistedAsync(jti, context.HttpContext.RequestAborted))
                                context.Fail(Resources.ErrorAccessTokenRevoked);
                        }
                    };
                });

            services.AddAuthorization();

            return services;
        }

        public void AddApplicationServices()
        {
            services.AddHealthChecks();

            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ILessonService, LessonService>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAdminService, AdminService>();

            services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        }

        private void AddValidatedOptions<TOptions>() where TOptions : class
        {
            services.AddOptions<TOptions>()
                .BindConfiguration(typeof(TOptions).Name)
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }
    }

    public async static Task InitializeDevelopmentDatabaseAsync(this IServiceProvider services, CancellationToken ct)
    {
        await using var scope = services.CreateAsyncScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();

        await using var context = await factory.CreateDbContextAsync(ct);
        await context.Database.MigrateAsync(ct);

        await DbInitializer.SeedUsersAsync(factory, ct);
        await DbInitializer.SeedCoursesAsync(factory, ct);
    }
}