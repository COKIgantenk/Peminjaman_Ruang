using System.ComponentModel.DataAnnotations;

namespace PeminjamanRuangAPI.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public required string Email { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public required string Password { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(150)]
        public required string FullName { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(20)]
        public required string PhoneNumber { get; set; }

        [Range(1, int.MaxValue)]
        public int DepartmentId { get; set; }
    }
}