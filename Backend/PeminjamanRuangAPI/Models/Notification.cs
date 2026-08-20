namespace PeminjamanRuangAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? BookingId { get; set; }
        public required string NotificationType { get; set; } // BOOKING_APPROVED, BOOKING_REJECTED, BOOKING_CANCELLED
        public bool EmailSent { get; set; }
        public DateTime? SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
        
    }
}