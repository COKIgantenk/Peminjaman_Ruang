using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();
        Task<Department?> GetDepartmentByIdAsync(int id);
        Task<int> CreateDepartmentAsync(Department department);
    }
}