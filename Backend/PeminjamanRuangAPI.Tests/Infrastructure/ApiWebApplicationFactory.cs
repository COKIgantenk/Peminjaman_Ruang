using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace PeminjamanRuangAPI.Tests.Infrastructure
{
    public sealed class ApiWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration(
                (_, configuration) =>
                {
                    configuration.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["JwtSettings:SecretKey"] =
                                "AUTOMATED_TEST_SECRET_KEY_12345678901234567890",

                            ["JwtSettings:Issuer"] =
                                "PeminjamanRuangAPI",

                            ["JwtSettings:Audience"] =
                                "PeminjamanRuangClient",

                            ["JwtSettings:ExpirationMinutes"] =
                                "60",

                            ["ConnectionStrings:DefaultConnection"] =
                                "Host=localhost;Port=5432;Database=test;Username=test;Password=test"
                        });
                });

            builder.ConfigureServices(services =>
            {
                // Background workers tidak perlu berjalan
                // pada authorization smoke tests.
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

                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme =
                            TestAuthHandler.SchemeName;

                        options.DefaultChallengeScheme =
                            TestAuthHandler.SchemeName;

                        options.DefaultForbidScheme =
                            TestAuthHandler.SchemeName;
                    })
                    .AddScheme<
                        AuthenticationSchemeOptions,
                        TestAuthHandler>(
                            TestAuthHandler.SchemeName,
                            _ => { });
            });
        }
    }
}