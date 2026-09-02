using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PeminjamanRuangAPI.Tests.Helpers
{
    public static class JwtTestTokenFactory
    {
        public static string CreateToken(
            int userId,
            string email,
            string fullName,
            string role,
            DateTime expiresUtc)
        {
            var claims = new List<Claim>
            {
                new(
                    JwtRegisteredClaimNames.Sub,
                    userId.ToString()),

                new(
                    JwtRegisteredClaimNames.Email,
                    email),

                new(
                    ClaimTypes.Name,
                    fullName),

                new(
                    ClaimTypes.Role,
                    role)
            };

            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        Infrastructure
                            .RealJwtWebApplicationFactory
                            .SecretKey));

            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);

            var token =
                new JwtSecurityToken(
                    issuer:
                        Infrastructure
                            .RealJwtWebApplicationFactory
                            .Issuer,

                    audience:
                        Infrastructure
                            .RealJwtWebApplicationFactory
                            .Audience,

                    claims: claims,

                    notBefore:
                        DateTime.UtcNow.AddMinutes(-5),

                    expires:
                        expiresUtc,

                    signingCredentials:
                        credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}