using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PeminjamanRuangAPI.HealthChecks
{
    public static class HealthCheckResponseWriter
    {
        public static async Task WriteResponse(
            HttpContext context,
            HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                totalDuration =
                    report.TotalDuration.TotalMilliseconds,
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    duration =
                        entry.Value.Duration.TotalMilliseconds
                })
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    response,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy =
                            JsonNamingPolicy.CamelCase
                    }));
        }
    }
}