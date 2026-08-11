namespace PeminjamanRuangAPI.Models
{
    public class Maintenance
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string MaintenanceCategory { get; set; }
        public string PriorityLevel { get; set; } // LOW, MEDIUM, HIGH
        public string FacilitiesServiced { get; set; }
        public string Documentation { get; set; }
        public string Description { get; set; }
        public int CreatedByAdminId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}