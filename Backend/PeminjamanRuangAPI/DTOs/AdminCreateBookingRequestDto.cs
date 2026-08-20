namespace PeminjamanRuangAPI.DTOs
{
    public class AdminCreateBookingRequestDto
    {
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
    }
}