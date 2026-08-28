using Dapper;
using System.Text;
using System.Security.Claims;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Services;
using PeminjamanRuangAPI.Exceptions;
using PeminjamanRuangAPI.Repositories;
using PeminjamanRuangAPI.Configuration;
using PeminjamanRuangAPI.Data.TypeHandlers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

// Add CORS if needed for API calls
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

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

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.MapGet("/api/test-db", async (DatabaseConnection db) =>
{
    using var connection = db.CreateConnection();

    var result = await connection.ExecuteScalarAsync<int>("SELECT 1");

    return Results.Ok(new
    {
        success = result == 1,
        message = "Database connection berhasil."
    });
});

app.Run();