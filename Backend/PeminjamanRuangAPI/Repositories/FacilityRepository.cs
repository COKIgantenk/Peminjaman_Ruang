using Dapper;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public class FacilityRepository : IFacilityRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public FacilityRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Facility>> GetAllFacilitiesAsync()
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    name AS ""Name"",
                    description AS ""Description"",
                    created_at AS ""CreatedAt"",
                    updated_at AS ""UpdatedAt""
                FROM facilities
                ORDER BY id";

            return await connection.QueryAsync<Facility>(query);
        }

        public async Task<Facility?> GetFacilityByIdAsync(int id)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    name AS ""Name"",
                    description AS ""Description"",
                    created_at AS ""CreatedAt"",
                    updated_at AS ""UpdatedAt""
                FROM facilities
                WHERE id = @Id";

            return await connection.QueryFirstOrDefaultAsync<Facility>(
                query,
                new { Id = id }
            );
        }

        public async Task<bool> CreateFacilityAsync(Facility facility)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                INSERT INTO facilities
                    (name, description, created_at, updated_at)
                VALUES
                    (@Name, @Description, NOW(), NOW())";

            var result = await connection.ExecuteAsync(query, facility);

            return result > 0;
        }

        public async Task<bool> UpdateFacilityAsync(Facility facility)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                UPDATE facilities
                SET
                    name = @Name,
                    description = @Description,
                    updated_at = NOW()
                WHERE id = @Id";

            var result = await connection.ExecuteAsync(query, facility);

            return result > 0;
        }

        public async Task<bool> DeleteFacilityAsync(int id)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                DELETE FROM facilities
                WHERE id = @Id";

            var result = await connection.ExecuteAsync(
                query,
                new { Id = id }
            );

            return result > 0;
        }
    }
}