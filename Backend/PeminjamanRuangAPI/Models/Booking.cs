namespace PeminjamanRuangAPI.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoomId { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int NumPeople { get; set; }
        public string Title { get; set; }
        public string RequesterName { get; set; }
        public string RequesterDivision { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // PENDING, APPROVED, REJECTED, DECLINED, CANCELLED
        public string ApprovalNotes { get; set; }
        public int? ApprovedByAdminId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}