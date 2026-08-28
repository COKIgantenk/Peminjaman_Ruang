using System.ComponentModel.DataAnnotations;

namespace PeminjamanRuangAPI.Configuration
{
    public class JwtSettings
    {
        [Required]
        [MinLength(32)]
        public string SecretKey { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int ExpirationMinutes { get; set; }
    }
}