using Dapper;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public DepartmentRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT id, name, created_at, updated_at
                    FROM departments
                    ORDER BY id";

                return await connection.QueryAsync<Department>(query);
            }
        }

        public async Task<Department> GetDepartmentByIdAsync(int id)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT id, name, created_at, updated_at
                    FROM departments
                    WHERE id = @Id";

                return await connection.QueryFirstOrDefaultAsync<Department>(
                    query,
                    new { Id = id }
                );
            }
        }

        public async Task<bool> CreateDepartmentAsync(Department department)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    INSERT INTO departments (name, created_at, updated_at)
                    VALUES (@Name, NOW(), NOW())";

                var result = await connection.ExecuteAsync(query, department);

                return result > 0;
            }
        }
    }
}