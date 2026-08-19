using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IFacilityRepository
    {
        Task<IEnumerable<Facility>> GetAllFacilitiesAsync();

        Task<Facility?> GetFacilityByIdAsync(int id);

        Task<bool> CreateFacilityAsync(Facility facility);

        Task<bool> UpdateFacilityAsync(Facility facility);

        Task<bool> DeleteFacilityAsync(int id);
    }
}