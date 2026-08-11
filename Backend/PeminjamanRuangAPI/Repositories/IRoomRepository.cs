using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task<Room> GetRoomByIdAsync(int id);
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime bookingDate, TimeSpan startTime, TimeSpan endTime, int capacity);
        Task<bool> CreateRoomAsync(Room room);
        Task<bool> UpdateRoomAsync(Room room);
        Task<bool> DeleteRoomAsync(int id);
        Task<IEnumerable<Facility>> GetRoomFacilitiesAsync(int roomId);
        Task<bool> AddFacilityToRoomAsync(int roomId, int facilityId);
        Task<bool> RemoveFacilityFromRoomAsync(int roomId, int facilityId);
    }
}