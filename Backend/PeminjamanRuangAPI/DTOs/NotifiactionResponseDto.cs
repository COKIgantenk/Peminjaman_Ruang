namespace PeminjamanRuangAPI.DTOs
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? BookingId { get; set; }

        public string NotificationType { get; set; } = string.Empty;

        public bool EmailSent { get; set; }

        public DateTime? SentAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}