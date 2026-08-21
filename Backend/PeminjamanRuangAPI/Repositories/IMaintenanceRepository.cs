using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IMaintenanceRepository
    {
        Task<IEnumerable<Maintenance>> GetAllMaintenancesAsync();

        Task<Maintenance?> GetMaintenanceByIdAsync(int id);

        Task<IEnumerable<Maintenance>> GetRoomMaintenancesAsync(int roomId);

        Task<int> CreateMaintenanceAsync(Maintenance maintenance);

        Task<bool> CompleteMaintenanceAsync(int id);
    }
}