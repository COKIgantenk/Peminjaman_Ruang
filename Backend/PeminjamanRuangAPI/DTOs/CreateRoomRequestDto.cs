namespace PeminjamanRuangAPI.DTOs
{
    public class CreateRoomRequestDto
    {
        public required string Name { get; set; }

        public required string Location { get; set; }

        public int Capacity { get; set; }

        public required string Description { get; set; }

        public string? ImageUrl { get; set; }
    }
}