namespace PeminjamanRuangAPI.DTOs
{
    public class MaintenanceResponseDto
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public string MaintenanceCategory { get; set; } = string.Empty;

        public string PriorityLevel { get; set; } = string.Empty;

        public string? FacilitiesServiced { get; set; }

        public string? Documentation { get; set; }

        public string Description { get; set; } = string.Empty;

        public int CreatedByAdminId { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}