using System.Net;
using System.Net.Http.Headers;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Tests.Helpers;
using PeminjamanRuangAPI.Tests.Infrastructure;

namespace PeminjamanRuangAPI.Tests.Security
{
    public sealed class JwtSecurityTests
        : IClassFixture<RealJwtWebApplicationFactory>
    {
        private readonly RealJwtWebApplicationFactory
            _factory;

        public JwtSecurityTests(
            RealJwtWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task InvalidJwt_Returns401()
        {
            using var client =
                _factory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    "this-is-not-a-valid-jwt");

            var response =
                await client.GetAsync("/api/User");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task ExpiredJwt_Returns401()
        {
            var user =
                CreateUser(
                    role: "ADMIN",
                    isActive: true);

            _factory.UserRepository.SetUser(user);

            var token =
                JwtTestTokenFactory.CreateToken(
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.Role,
                    DateTime.UtcNow.AddMinutes(-1));

            using var client =
                CreateAuthenticatedClient(token);

            var response =
                await client.GetAsync("/api/User");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task ActiveUserJwt_OnAdminEndpoint_Returns403()
        {
            var user =
                CreateUser(
                    role: "USER",
                    isActive: true);

            _factory.UserRepository.SetUser(user);

            var token =
                JwtTestTokenFactory.CreateToken(
                    user.Id,
                    user.Email,
                    user.FullName,
                    "USER",
                    DateTime.UtcNow.AddMinutes(10));

            using var client =
                CreateAuthenticatedClient(token);

            var response =
                await client.GetAsync("/api/User");

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task InactiveUser_WithPreviouslyIssuedJwt_Returns401()
        {
            var user =
                CreateUser(
                    role: "USER",
                    isActive: false);

            _factory.UserRepository.SetUser(user);

            var token =
                JwtTestTokenFactory.CreateToken(
                    user.Id,
                    user.Email,
                    user.FullName,
                    "USER",
                    DateTime.UtcNow.AddMinutes(10));

            using var client =
                CreateAuthenticatedClient(token);

            var response =
                await client.GetAsync("/api/Booking/my");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task DeletedUser_WithPreviouslyIssuedJwt_Returns401()
        {
            var user =
                CreateUser(
                    role: "USER",
                    isActive: true);

            var token =
                JwtTestTokenFactory.CreateToken(
                    user.Id,
                    user.Email,
                    user.FullName,
                    "USER",
                    DateTime.UtcNow.AddMinutes(10));

            // Simulasikan user sudah soft-delete:
            // GetUserByIdAsync tidak lagi menemukan user.
            _factory.UserRepository.SetUser(null);

            using var client =
                CreateAuthenticatedClient(token);

            var response =
                await client.GetAsync("/api/Booking/my");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task RoleChanged_AfterJwtIssued_Returns401()
        {
            var currentUser =
                CreateUser(
                    role: "USER",
                    isActive: true);

            _factory.UserRepository.SetUser(
                currentUser);

            // Token lama masih membawa role ADMIN.
            var oldToken =
                JwtTestTokenFactory.CreateToken(
                    currentUser.Id,
                    currentUser.Email,
                    currentUser.FullName,
                    "ADMIN",
                    DateTime.UtcNow.AddMinutes(10));

            using var client =
                CreateAuthenticatedClient(oldToken);

            var response =
                await client.GetAsync("/api/User");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        private HttpClient CreateAuthenticatedClient(
            string token)
        {
            var client =
                _factory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            return client;
        }

        private static User CreateUser(
            string role,
            bool isActive)
        {
            return new User
            {
                Id = 1001,
                Email = "jwt.test@example.com",
                PasswordHash = "not-used",
                FullName = "JWT Test User",
                PhoneNumber = "081234567890",
                DepartmentId = 1,
                Role = role,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DeletedAt = null
            };
        }
    }
}