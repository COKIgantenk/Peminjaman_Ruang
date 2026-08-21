namespace PeminjamanRuangAPI.DTOs
{
    public class CreateMaintenanceRequestDto
    {
        public int RoomId { get; set; }

        public required string MaintenanceCategory { get; set; }

        public required string PriorityLevel { get; set; }

        public string? FacilitiesServiced { get; set; }

        public string? Documentation { get; set; }

        public required string Description { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}