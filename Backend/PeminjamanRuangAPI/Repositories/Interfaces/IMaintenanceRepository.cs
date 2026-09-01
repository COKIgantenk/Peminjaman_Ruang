using Npgsql;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IMaintenanceRepository
    {
        Task<IEnumerable<Maintenance>> GetAllMaintenancesAsync();

        Task<Maintenance?> GetMaintenanceByIdAsync(int id);

        Task<IEnumerable<Maintenance>> GetRoomMaintenancesAsync(int roomId);

        Task<IEnumerable<Maintenance>> GetMaintenancesReadyToActivateAsync();

        Task<IEnumerable<Maintenance>> GetMaintenancesReadyToCompleteAsync();

        Task<bool> CompleteScheduledMaintenanceWithStatusAsync(
            int maintenanceId,
            int roomId);

        Task<bool> ActivateMaintenanceWithStatusAsync(
            int maintenanceId,
            int roomId,
            int adminId,
            string reason);

        Task<int> CreateMaintenanceAsync(Maintenance maintenance);

        Task<bool> CompleteMaintenanceAsync(int id);

        Task<int> CreateMaintenanceWithStatusAsync(
            Maintenance maintenance,
            string reason);

        Task<bool> CompleteMaintenanceWithStatusAsync(
            int maintenanceId,
            int roomId,
            int adminId);

        Task<bool> CompleteMaintenanceWithStatusAsync(
            int maintenanceId,
            int roomId,
            int adminId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);

        Task<int> CreateMaintenanceWithStatusAsync(
            Maintenance maintenance,
            string reason,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);

        Task<bool> HasMaintenanceConflictAsync(
            int roomId,
            DateOnly bookingDate);

        Task<bool> HasMaintenanceConflictAsync(
            int roomId,
            DateOnly bookingDate,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);

        Task<bool> HasMaintenanceScheduleConflictAsync(
            int roomId,
            DateOnly startDate,
            DateOnly? endDate);
        
        Task<bool> HasMaintenanceScheduleConflictAsync(
            int roomId,
            DateOnly startDate,
            DateOnly? endDate,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
    }
}