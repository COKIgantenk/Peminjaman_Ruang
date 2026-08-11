using Dapper;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Register DatabaseConnection
builder.Services.AddScoped<DatabaseConnection>();

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapRazorPages();

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