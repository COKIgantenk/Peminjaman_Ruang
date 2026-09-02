using System.ComponentModel.DataAnnotations;

namespace PeminjamanRuangAPI.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public required string Email { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Password { get; set; }
    }
}