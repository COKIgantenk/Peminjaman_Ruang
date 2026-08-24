namespace PeminjamanRuangAPI.Models
{
    public class Maintenance
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public required string MaintenanceCategory { get; set; }
        public required string PriorityLevel { get; set; } // LOW, MEDIUM, HIGH
        public string? FacilitiesServiced { get; set; }
        public string? Documentation { get; set; }
        public required string Description { get; set; }
        public int CreatedByAdminId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
    }
}