using Dapper;
using System.Text;
using System.Security.Claims;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Services;
using PeminjamanRuangAPI.Exceptions;
using PeminjamanRuangAPI.HealthChecks;
using PeminjamanRuangAPI.Repositories;
using PeminjamanRuangAPI.Configuration;
using PeminjamanRuangAPI.Data.TypeHandlers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    // Untuk environment reverse proxy/cloud seperti Railway.
    // Proxy address tidak selalu statis, jadi jangan batasi ke localhost saja.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services
    .AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(
            "API berjalan."),
        tags: new[] { "live" })
    .AddCheck<DatabaseHealthCheck>(
        "database",
        tags: new[] { "ready" });

// Register DatabaseConnection
builder.Services.AddScoped<DatabaseConnection>();

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IFacilityRepository, FacilityRepository>();
builder.Services.AddScoped< IBookingCancellationRepository, BookingCancellationRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IRoomStatusHistoryRepository, RoomStatusHistoryRepository>();
builder.Services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
builder.Services.AddScoped<IRoomCleaningSessionRepository, RoomCleaningSessionRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();


// Register Services
builder.Services.AddScoped<PasswordService>();
builder.Services.AddHostedService<BookingCleaningBackgroundService>();
builder.Services.AddHostedService<MaintenanceActivationBackgroundService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<BookingTransactionService>();
builder.Services.AddScoped<MaintenanceTransactionService>();
builder.Services.AddScoped<RoomStatusTransactionService>(); 
builder.Services.AddScoped<CleaningTransactionService>();
builder.Services.AddScoped<UserTransactionService>();
builder.Services.AddScoped<RoomTransactionService>();
builder.Services.AddScoped<FacilityTransactionService>();
builder.Services.AddScoped<DepartmentTransactionService>();

builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("JwtSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<JwtSettings>
    >().Value
);

builder.Services.AddScoped<JwtService>();

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings belum dikonfigurasi.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
            ),

            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdClaim =
                    context.Principal?
                        .FindFirst(ClaimTypes.NameIdentifier)?
                        .Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    context.Fail("User ID pada token tidak valid.");
                    return;
                }

                var userRepository =
                    context.HttpContext.RequestServices
                        .GetRequiredService<IUserRepository>();

                var user =
                    await userRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    context.Fail("User tidak ditemukan.");
                    return;
                }

                if (!user.IsActive)
                {
                    context.Fail("Akun user sudah tidak aktif.");
                    return;
                }

                var tokenRole =
                    context.Principal?
                        .FindFirst(ClaimTypes.Role)?
                        .Value;

                if (string.IsNullOrWhiteSpace(tokenRole) ||
                    !string.Equals(
                        tokenRole,
                        user.Role,
                        StringComparison.OrdinalIgnoreCase))
                {
                    context.Fail("Role user sudah berubah.");
                    return;
                }
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, CancellationToken) =>
    {
        context.HttpContext.Response.StatusCode =
            StatusCodes.Status429TooManyRequests;

        context.HttpContext.Response.ContentType =
            "application.problem+json";

        if (context.Lease.TryGetMetadata(
                MetadataName.RetryAfter,
                out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                Math.Ceiling(retryAfter.TotalSeconds).ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://httpstatuses.com/429",
                title = "Too Many Requests",
                status = 429,
                detail = "Terlalu banyak percobaan. Silakan coba lagi beberapa saat."
            },
            CancellationToken);
    };

    options.AddPolicy("AuthPolicy", httpContext =>
    {
        var clientIp =
            httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: _=> new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

// Add CORS if needed for API calls

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseForwardedHeaders();

//Global exception handler
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("FrontendPolicy");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate =
            registration =>
                registration.Tags.Contains("live"),

        ResponseWriter =
            HealthCheckResponseWriter.WriteResponse
    });

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate =
            registration =>
                registration.Tags.Contains("ready"),

        ResponseWriter =
            HealthCheckResponseWriter.WriteResponse
    });

app.Run();