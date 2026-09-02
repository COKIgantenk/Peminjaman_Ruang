using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PeminjamanRuangAPI.Tests.Infrastructure
{
    public sealed class TestAuthHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Test";

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(
                    "X-Test-User-Role",
                    out var roleHeader))
            {
                return Task.FromResult(
                    AuthenticateResult.NoResult());
            }

            var role = roleHeader.ToString();

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    "999999"),

                new Claim(
                    ClaimTypes.Name,
                    "Automated Test User"),

                new Claim(
                    ClaimTypes.Role,
                    role)
            };

            var identity = new ClaimsIdentity(
                claims,
                SchemeName);

            var principal =
                new ClaimsPrincipal(identity);

            var ticket =
                new AuthenticationTicket(
                    principal,
                    SchemeName);

            return Task.FromResult(
                AuthenticateResult.Success(ticket));
        }
    }
}