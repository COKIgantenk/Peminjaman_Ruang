using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PeminjamanRuangAPI.Data;

namespace PeminjamanRuangAPI.HealthChecks
{
    public sealed class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DatabaseHealthCheck(
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var databaseConnection =
                    scope.ServiceProvider
                        .GetRequiredService<DatabaseConnection>();

                await using var connection =
                    databaseConnection.CreateConnection();

                await connection.OpenAsync(cancellationToken);

                var result =
                    await connection.ExecuteScalarAsync<int>(
                        new CommandDefinition(
                            "SELECT 1",
                            cancellationToken: cancellationToken));

                return result == 1
                    ? HealthCheckResult.Healthy(
                        "Database PostgreSQL dapat diakses.")
                    : HealthCheckResult.Unhealthy(
                        "Database PostgreSQL memberikan response yang tidak valid.");
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                return HealthCheckResult.Unhealthy(
                    "Pemeriksaan database dibatalkan.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Database PostgreSQL tidak dapat diakses.",
                    ex);
            }
        }
    }
}