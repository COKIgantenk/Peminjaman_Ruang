namespace PeminjamanRuangAPI.DTOs
{
    public class RoomStatusHistoryResponseDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public int? ChangedByAdminId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}