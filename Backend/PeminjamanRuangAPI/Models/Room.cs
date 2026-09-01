namespace PeminjamanRuangAPI.Models
{
    public class Room
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Location { get; set; }
        public int Capacity { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}