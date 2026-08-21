namespace PeminjamanRuangAPI.DTOs
{
    public class ChangeRoomStatusRequestDto
    {
        public required string Status { get; set; }

        public string? Reason { get; set; }
    }
}