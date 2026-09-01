using System.ComponentModel.DataAnnotations;

namespace PeminjamanRuangAPI.DTOs
{
    public class AdminCreateBookingRequestDto
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Range(1, int.MaxValue)]
        public int RoomId { get; set; }

        public DateOnly BookingDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        [Range(1, int.MaxValue)]
        public int NumPeople { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 1)]
        public required string RequesterName { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 1)]
        public required string RequesterDivision { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }
    }
}