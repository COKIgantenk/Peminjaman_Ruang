using System.ComponentModel.DataAnnotations;

namespace PeminjamanRuangAPI.DTOs
{
    public class CancelBookingRequestDto
    {
        [Required]
        [StringLength(500, MinimumLength = 1)]
        public required string Reason { get; set; }
    }
}