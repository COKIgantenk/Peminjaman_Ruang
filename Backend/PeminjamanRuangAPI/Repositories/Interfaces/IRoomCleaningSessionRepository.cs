using Npgsql;   
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IRoomCleaningSessionRepository
    {
        Task<IEnumerable<RoomCleaningSession>> GetAllCleaningSessionsAsync();

        Task<RoomCleaningSession?> GetCleaningSessionByIdAsync(int id);

        Task<IEnumerable<RoomCleaningSession>> GetRoomCleaningSessionsAsync(
            int roomId);

        Task<int> CreateAutomaticCleaningSessionAsync(
            int rooId,
            int bookingId);

        Task<bool> SetCleaningDurationAsync(
            int cleaningSessionId,
            string cleaningDuration,
            int? customDurationMinutes);

        Task<bool> SetCleaningDurationAsync(
            int cleaningSessionId,
            string cleaningDuration,
            int? customDurationMinutes,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);

        Task<IEnumerable<RoomCleaningSession>>
            GetCleaningSessionsReadyToCompleteAsync();

        Task<bool> CompleteAutomaticCleaningWithStatusAsync(
            int cleaningSessionId,
            int roomId);
            
        Task<RoomCleaningSession?> GetCleaningSessionByIdForUpdateAsync(
            int id,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
    }
}