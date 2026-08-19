namespace PeminjamanRuangAPI.Models
{
    public class BookingCancellation
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public required string CancellationReason { get; set; }
        public int CancelledByUserId { get; set; }
        public DateTime CancelledAt { get; set; }
    }
}
