using System.ComponentModel.DataAnnotations;

namespace PeminjamanRuangAPI.DTOs
{
    public class SetCleaningDurationRequestDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public required string CleaningDuration { get; set; }

        [Range(1, 1440)]
        public int? CustomDurationMinutes { get; set; }
    }
}