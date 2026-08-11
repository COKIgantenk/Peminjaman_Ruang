namespace PeminjamanRuangAPI.Models
{
    public class RoomCleaningSession
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int? BookingId { get; set; }
        public string CleaningDuration { get; set; } // 10_MINUTES, 20_MINUTES, 30_MINUTES, CUSTOM
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}