namespace PeminjamanRuangAPI.Models
{
    public class RoomStatusHistory
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string Status { get; set; } // ACTIVE, OUT_OF_SERVICE, MAINTENANCE, CLEANING
        public string Reason { get; set; }
        public int? ChangedByAdminId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}