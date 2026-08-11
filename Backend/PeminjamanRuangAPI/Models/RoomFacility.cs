namespace PeminjamanRuangAPI.Models
{
    public class RoomFacility
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int FacilityId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}