namespace PeminjamanRuangAPI.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoomId { get; set; }
        public DateOnly BookingDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int NumPeople { get; set; }
        public string? Title { get; set; }
        public required string RequesterName { get; set; }
        public required string RequesterDivision { get; set; }
        public string? Description { get; set; }
        public required string Status { get; set; } // PENDING, APPROVED, REJECTED, CANCELLED
        public string? ApprovalNotes { get; set; }
        public int? ApprovedByAdminId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}