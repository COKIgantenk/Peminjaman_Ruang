namespace PeminjamanRuangAPI.DTOs
{
    public class BookingResponseDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int RoomId { get; set; }

        public DateOnly BookingDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public int NumPeople { get; set; }

        public string? Title { get; set; }

        public string RequesterName { get; set; } = string.Empty;

        public string RequesterDivision { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? ApprovalNotes { get; set; }

        public int? ApprovedByAdminId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}