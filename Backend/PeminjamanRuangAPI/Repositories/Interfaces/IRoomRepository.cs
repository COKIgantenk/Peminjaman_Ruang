using PeminjamanRuangAPI.Models;
using Npgsql;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task<Room?> GetRoomByIdAsync(int id);
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(
            DateTime bookingDate, 
            TimeSpan startTime, 
            TimeSpan endTime, 
            int capacity,
            int[]? facilityIds);
        Task<int> CreateRoomAsync(Room room);
        Task<int> CreateRoomAsync(
            Room room,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool> UpdateRoomAsync(Room room);
        Task<bool> UpdateRoomAsync(
            Room room,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool> DeactivateRoomAsync(int id);
        Task<IEnumerable<Facility>> GetRoomFacilitiesAsync(int roomId);
        Task<bool> AddFacilityToRoomAsync(int roomId, int facilityId);
        Task<bool> RemoveFacilityFromRoomAsync(int roomId, int facilityId);
    }
}