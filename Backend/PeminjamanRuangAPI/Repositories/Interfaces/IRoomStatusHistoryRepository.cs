using Npgsql;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IRoomStatusHistoryRepository
    {
        Task<IEnumerable<RoomStatusHistory>> GetRoomStatusHistoryAsync(
            int roomId);

        Task<RoomStatusHistory?> GetLatestRoomStatusAsync(
            int roomId);

        Task<RoomStatusHistory?> GetLatestRoomStatusAsync(
            int roomId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);

        Task<bool> LockRoomAsync(
            int roomId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);

        Task<bool> CreateRoomStatusHistoryAsync(
            RoomStatusHistory roomStatusHistory);

        Task<bool> CreateRoomStatusHistoryAsync(
            RoomStatusHistory roomStatusHistory,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);

        Task<bool> ChangeRoomStatusAsync(
            int roomId,
            string Status,
            string? reason,
            int adminId);

        Task<bool> ChangeRoomStatusAsync(
            int roomId,
            string status,
            string? reason,
            int adminId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
    }
}