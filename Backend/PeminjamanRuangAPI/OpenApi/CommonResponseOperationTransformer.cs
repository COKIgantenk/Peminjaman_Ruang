using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PeminjamanRuangAPI.OpenApi
{
    public sealed class CommonResponseOperationTransformer
        : IOpenApiOperationTransformer
    {
        public Task TransformAsync(
            OpenApiOperation operation,
            OpenApiOperationTransformerContext context,
            CancellationToken cancellationToken)
        {
            var metadata =
                context.Description
                    .ActionDescriptor
                    .EndpointMetadata;

            var allowAnonymous =
                metadata
                    .OfType<AllowAnonymousAttribute>()
                    .Any();

            var authorizeData =
                metadata
                    .OfType<IAuthorizeData>()
                    .ToArray();

            operation.Responses ??=
                new OpenApiResponses();

            // Semua API controller dapat menghasilkan 400
            // dari model validation / invalid request.
            AddResponseIfMissing(
                operation,
                "400",
                "Request tidak valid.");

            // Endpoint yang membutuhkan authentication.
            if (!allowAnonymous && authorizeData.Length > 0)
            {
                AddResponseIfMissing(
                    operation,
                    "401",
                    "Authentication diperlukan atau token tidak valid.");

                var requiresRole =
                    authorizeData.Any(
                        data =>
                            !string.IsNullOrWhiteSpace(data.Roles));

                if (requiresRole)
                {
                    AddResponseIfMissing(
                        operation,
                        "403",
                        "User tidak memiliki role/izin yang diperlukan.");
                }
            }

            // Global exception handler dapat menghasilkan 500.
            AddResponseIfMissing(
                operation,
                "500",
                "Terjadi kesalahan internal pada server.");

            return Task.CompletedTask;
        }

        private static void AddResponseIfMissing(
            OpenApiOperation operation,
            string statusCode,
            string description)
        {
            operation.Responses ??=
                new OpenApiResponses();

            if (operation.Responses.ContainsKey(statusCode))
            {
                return;
            }

            operation.Responses[statusCode] =
                new OpenApiResponse
                {
                    Description = description
                };
        }
    }
}