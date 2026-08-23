namespace PeminjamanRuangAPI.DTOs
{
    public class RoomCleaningSessionResponseDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int? BookingId { get; set; }
        public string? CleaningDuration { get; set; } 
        public int? CustomDurationMinutes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}