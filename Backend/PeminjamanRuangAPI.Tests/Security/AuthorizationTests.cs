using System.Net;
using PeminjamanRuangAPI.Tests.Infrastructure;

namespace PeminjamanRuangAPI.Tests.Security
{
    public sealed class AuthorizationTests
        : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly ApiWebApplicationFactory _factory;

        public AuthorizationTests(
            ApiWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/api/Booking/my")]
        [InlineData("/api/Room")]
        [InlineData("/api/Facility")]
        [InlineData("/api/Notification/my")]
        [InlineData("/api/RoomStatus/1/latest")]
        public async Task ProtectedEndpoint_WithoutAuthentication_Returns401(
            string url)
        {
            using var client =
                _factory.CreateClient();

            var response =
                await client.GetAsync(url);

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Theory]
        [InlineData("/api/User")]
        [InlineData("/api/Maintenance")]
        [InlineData("/api/RoomCleaning")]
        public async Task AdminEndpoint_WithUserRole_Returns403(
            string url)
        {
            using var client =
                _factory.CreateClient();

            client.DefaultRequestHeaders.Add(
                "X-Test-User-Role",
                "USER");

            var response =
                await client.GetAsync(url);

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task Login_WithoutAuthentication_IsPublic()
        {
            using var client =
                _factory.CreateClient();
        
            using var content =
                new StringContent(
                    "{}",
                    System.Text.Encoding.UTF8,
                    "application/json");
        
            var response =
                await client.PostAsync(
                    "/api/Auth/login",
                    content);
        
            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        }
        
        [Fact]
        public async Task Register_WithoutAuthentication_IsPublic()
        {
            using var client =
                _factory.CreateClient();
        
            using var content =
                new StringContent(
                    "{}",
                    System.Text.Encoding.UTF8,
                    "application/json");
        
            var response =
                await client.PostAsync(
                    "/api/Auth/register",
                    content);
        
            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        }
    }
}