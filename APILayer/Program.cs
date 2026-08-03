using System.Text;
using System.Threading.RateLimiting;
using APILayer.Helpers;
using APILayer.Middleware;
using ApplicationLayer;
using ApplicationLayer.Auth;
using InfrastructureLayer;
using InfrastructureLayer.Database;
using InfrastructureLayer.Interfaces;
using InfrastructureLayer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Resend;

namespace APILayer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
                var builder = WebApplication.CreateBuilder(args);

            // ── Application & Infrastructure ───────────────────────────────────────
            // Registers MediatR, AutoMapper, FluentValidation, and pipeline behaviors
            builder.Services.AddApplication();

            // Registers AppDbContext, all repositories, UserContextService, AuthService
            builder.Services.AddInfrastructure(builder.Configuration);

            // ── JWT Authentication ─────────────────────────────────────────────────
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // ── API ────────────────────────────────────────────────────────────────
            builder.Services.AddControllers();

            // Replaces the default 400 validation response with OperationResult format
            builder.Services.AddCustomValidationResponse();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();

            // ── Rate Limiting ──────────────────────────────────────────────────────
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Global per-IP fixed-window limiter — applies to every request
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = builder.Configuration.GetValue("RateLimiting:Global:PermitLimit", 100),
                            Window = TimeSpan.FromSeconds(builder.Configuration.GetValue("RateLimiting:Global:WindowSeconds", 60)),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                // Stricter per-IP policy for login / register / forgot-password / reset-password
                options.AddPolicy("auth", context =>
                    CreatePerIpFixedWindowLimiter(
                        context,
                        builder.Configuration.GetValue("RateLimiting:Auth:PermitLimit", 10),
                        builder.Configuration.GetValue("RateLimiting:Auth:WindowSeconds", 60)));

                // Refresh-token is used by session bootstrap and retry flows, so it needs a
                // looser budget than login-style endpoints while still being IP-partitioned.
                options.AddPolicy("session", context =>
                    CreatePerIpFixedWindowLimiter(
                        context,
                        builder.Configuration.GetValue("RateLimiting:Session:PermitLimit", 30),
                        builder.Configuration.GetValue("RateLimiting:Session:WindowSeconds", 60)));

                // Upload/analysis endpoints are user actions, not auth attempts.
                options.AddPolicy("uploads", context =>
                    CreatePerIpFixedWindowLimiter(
                        context,
                        builder.Configuration.GetValue("RateLimiting:Uploads:PermitLimit", 30),
                        builder.Configuration.GetValue("RateLimiting:Uploads:WindowSeconds", 60)));
            });

            // ── Health Checks ──────────────────────────────────────────────────────
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>("database");

            // ───── Email Service ─────────────────────────────────────────────
            builder.Services.AddOptions();
            builder.Services.AddHttpClient<ResendClient>();
            builder.Services.Configure<ResendClientOptions>(options =>
            {
                options.ApiToken = builder.Configuration["Resend:ApiKey"]!;
            });

            builder.Services.AddTransient<IResend, ResendClient>();
            builder.Services.AddScoped<IEmailService, EmailService>();


            // ── CORS ───────────────────────────────────────────────────────────────
            // AllowCredentials() is required for the HttpOnly refresh token cookie to work
            var corsOrigins = builder.Configuration
                .GetSection("CorsOrigins").Get<string[]>()
                ?? new[] { "http://localhost:5174", "http://localhost:5174",  };

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(corsOrigins)
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // ── Build ──────────────────────────────────────────────────────────────
            var app = builder.Build();

            // ── Middleware pipeline (order matters) ────────────────────────────────

            // Must be first — wraps everything so no exception escapes as a raw 500 page
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                // Serves Swagger UI. Fetches the generated spec, injects the Bearer security
                // scheme into it client-side, then hands the modified spec to Swagger UI —
                // this gives the standard Authorize button and lock icons without needing
                // Microsoft.OpenApi type imports (which changed incompatibly in v2).
                app.MapGet("/swagger", () => Results.Content("""
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>SEEKAT API</title>
                        <meta charset="utf-8"/>
                        <meta name="viewport" content="width=device-width, initial-scale=1">
                        <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css">
                    </head>
                    <body>
                    <div id="swagger-ui"></div>
                    <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
                    <script>
                        fetch('/openapi/v1.json')
                            .then(r => r.json())
                            .then(spec => {
                                // Inject Bearer JWT security scheme so Swagger shows the Authorize button
                                spec.components = spec.components || {};
                                spec.components.securitySchemes = {
                                    Bearer: {
                                        type: 'http',
                                        scheme: 'bearer',
                                        bearerFormat: 'JWT',
                                        description: 'Paste the JWT token from POST /api/auth/login'
                                    }
                                };
                                spec.security = [{ Bearer: [] }];

                                SwaggerUIBundle({
                                    spec: spec,
                                    dom_id: '#swagger-ui',
                                    presets: [SwaggerUIBundle.presets.apis, SwaggerUIBundle.SwaggerUIStandalonePreset],
                                    layout: 'BaseLayout',
                                    persistAuthorization: true
                                });
                            });
                    </script>
                    </body>
                    </html>
                    """, "text/html"));
            }

            //app.UseHttpsRedirection();

            // Must be before UseAuthentication and UseAuthorization.
            // Must also be before UseStaticFiles so static files include CORS headers —
            // without this, axios blob fetches for images are blocked by the browser.
            app.UseCors("AllowFrontend");

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRateLimiter();

            app.MapControllers();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            durationMs = e.Value.Duration.TotalMilliseconds
                        })
                    });
                }
            }).DisableRateLimiting();

            await ApplyMigrationsAsync(app.Services);

            if (app.Environment.IsDevelopment())
                await SeedAdminAsync(app.Services);

            app.Run();
        }

        static async Task ApplyMigrationsAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        private static RateLimitPartition<string> CreatePerIpFixedWindowLimiter(
            HttpContext context,
            int permitLimit,
            int windowSeconds)
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromSeconds(windowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
        }

        static async Task SeedAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            await InfrastructureLayer.Seeding.DbSeeder.SeedAdminAsync(scope.ServiceProvider);
        }
    }
}
