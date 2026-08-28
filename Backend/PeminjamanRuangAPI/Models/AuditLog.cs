namespace PeminjamanRuangAPI.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public required string Action { get; set; } // CREATE, UPDATE, DELETE, APPROVE, REJECT
        public required string EntityType { get; set; } // BOOKING, ROOM, USER, MAINTENANCE
        public int EntityId { get; set; }
        public string? Changes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}