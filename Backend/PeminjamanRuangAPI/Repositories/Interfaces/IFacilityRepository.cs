using Npgsql;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IFacilityRepository
    {
        Task<IEnumerable<Facility>> GetAllFacilitiesAsync();

        Task<Facility?> GetFacilityByIdAsync(int id);

        Task<int> CreateFacilityAsync(Facility facility);
        Task<int> CreateFacilityAsync(
            Facility facility,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);

        Task<bool> UpdateFacilityAsync(Facility facility);
        Task<bool> UpdateFacilityAsync(
            Facility facility,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);

        Task<bool> DeleteFacilityAsync(int id);
        Task<bool> DeleteFacilityAsync(
            int id,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
    }
}