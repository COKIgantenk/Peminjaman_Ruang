using System.ComponentModel.DataAnnotations;

namespace PeminjamanRuangAPI.DTOs
{
    public class RejectBookingRequestDto
    {
        [Required]
        [StringLength(500, MinimumLength = 1)]
        public required string Reason { get; set; }
    }
}