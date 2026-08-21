using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IRoomStatusHistoryRepository
    {
        Task<IEnumerable<RoomStatusHistory>> GetRoomStatusHistoryAsync(
            int roomId);

        Task<RoomStatusHistory?> GetLatestRoomStatusAsync(
            int roomId);

        Task<bool> CreateRoomStatusHistoryAsync(
            RoomStatusHistory roomStatusHistory);

        Task<bool> ChangeRoomStatusAsync(
            int roomId,
            string Status,
            string? reason,
            int adminId);
    }
}