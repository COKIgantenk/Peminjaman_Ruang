using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PeminjamanRuangAPI.OpenApi
{
    public sealed class AuthOperationTransformer
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

            if (allowAnonymous)
            {
                return Task.CompletedTask;
            }

            var requiresAuthorization =
                metadata
                    .OfType<IAuthorizeData>()
                    .Any();

            if (!requiresAuthorization)
            {
                return Task.CompletedTask;
            }

            operation.Security ??=
                new List<OpenApiSecurityRequirement>();

            operation.Security.Add(
                new OpenApiSecurityRequirement
                {
                    [
                        new OpenApiSecuritySchemeReference(
                            "Bearer",
                            context.Document)
                    ] = []
                });

            return Task.CompletedTask;
        }
    }
}