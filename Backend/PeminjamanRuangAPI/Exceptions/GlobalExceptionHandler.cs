using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace PeminjamanRuangAPI.Exceptions
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Unhandled exception terjadi pada {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            var (statusCode, title, detail) = exception switch
            {
                PostgresException postgresException
                    when postgresException.SqlState
                        == PostgresErrorCodes.UniqueViolation
                    => (
                        StatusCodes.Status409Conflict,
                        "Conflict",
                        "Data yang sama sudah tersedia."
                    ),

                PostgresException postgresException
                    when postgresException.SqlState
                        == PostgresErrorCodes.ForeignKeyViolation
                    => (
                        StatusCodes.Status409Conflict,
                        "Conflict",
                        "Data tidak dapat diproses karena masih memiliki relasi dengan data lain."
                    ),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "Terjadi kesalahan pada server."
                )
            };

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            return true;
        }
    }
}