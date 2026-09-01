using System.ComponentModel.DataAnnotations;

namespace PeminjamanRuangAPI.DTOs
{
    public class CreateMaintenanceRequestDto
    {
        [Range(1, int.MaxValue)]
        public int RoomId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public required string MaintenanceCategory { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public required string PriorityLevel { get; set; }

        [StringLength(1000)]
        public string? FacilitiesServiced { get; set; }

        [StringLength(1000)]
        public string? Documentation { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public required string Description { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}