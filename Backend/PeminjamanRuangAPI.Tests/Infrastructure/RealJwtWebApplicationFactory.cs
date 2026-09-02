using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using PeminjamanRuangAPI.Configuration;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Tests.Infrastructure
{
    public sealed class RealJwtWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        public const string SecretKey =
            "AUTOMATED_TEST_SECRET_KEY_12345678901234567890";

        public const string Issuer =
            "PeminjamanRuangAPI";

        public const string Audience =
            "PeminjamanRuangClient";

        public FakeUserRepository UserRepository { get; } =
            new();

        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            // PENTING:
            // Ini diperlukan agar JwtSettings yang memakai
            // Bind + ValidateOnStart mendapatkan nilai valid.
            builder.ConfigureAppConfiguration(
                (_, configuration) =>
                {
                    configuration.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["JwtSettings:SecretKey"] =
                                SecretKey,

                            ["JwtSettings:Issuer"] =
                                Issuer,

                            ["JwtSettings:Audience"] =
                                Audience,

                            ["JwtSettings:ExpirationMinutes"] =
                                "60",

                            ["ConnectionStrings:DefaultConnection"] =
                                "Host=localhost;Port=5432;Database=test;Username=test;Password=test"
                        });
                });

            builder.ConfigureServices(services =>
            {
                // Background workers tidak perlu berjalan
                // pada automated security tests.
                var hostedServices =
                    services
                        .Where(descriptor =>
                            descriptor.ServiceType ==
                            typeof(IHostedService))
                        .ToList();

                foreach (var descriptor in hostedServices)
                {
                    services.Remove(descriptor);
                }

                // Gunakan fake user repository,
                // jangan PostgreSQL asli.
                services.RemoveAll<IUserRepository>();

                services.AddSingleton(
                    UserRepository);

                services.AddSingleton<IUserRepository>(
                    serviceProvider =>
                        serviceProvider
                            .GetRequiredService<FakeUserRepository>());

                // JwtService menerima JwtSettings test.
                services.RemoveAll<JwtSettings>();

                services.AddSingleton(
                    new JwtSettings
                    {
                        SecretKey = SecretKey,
                        Issuer = Issuer,
                        Audience = Audience,
                        ExpirationMinutes = 60
                    });

                // PENTING:
                // Program.cs membentuk JwtBearerOptions
                // dari jwtSettings yang dibaca lebih awal.
                // Override final options untuk test.
                services.PostConfigure<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.TokenValidationParameters =
                            new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,

                                ValidIssuer = Issuer,
                                ValidAudience = Audience,

                                IssuerSigningKey =
                                    new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(
                                            SecretKey)),

                                ClockSkew = TimeSpan.Zero
                            };
                    });
            });
        }
    }
}