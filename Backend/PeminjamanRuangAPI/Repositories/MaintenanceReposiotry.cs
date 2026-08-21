using Dapper;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public class MaintenanceRepository : IMaintenanceRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public MaintenanceRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Maintenance>> GetAllMaintenancesAsync()
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    room_id AS ""RoomId"",
                    maintenance_category AS ""MaintenanceCategory"",
                    priority_level AS ""PriorityLevel"",
                    facilities_serviced AS ""FacilitiesServiced"",
                    documentation AS ""Documentation"",
                    description AS ""Description"",
                    created_by_admin_id AS ""CreatedByAdminId"",
                    start_date AS ""StartDate"",
                    end_date AS ""EndDate"",
                    created_at AS ""CreatedAt"",
                    completed_at AS ""CompletedAt""
                FROM maintenance
                ORDER BY created_at DESC";

            return await connection.QueryAsync<Maintenance>(query);
        }

        public async Task<Maintenance?> GetMaintenanceByIdAsync(int id)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    room_id AS ""RoomId"",
                    maintenance_category AS ""MaintenanceCategory"",
                    priority_level AS ""PriorityLevel"",
                    facilities_serviced AS ""FacilitiesServiced"",
                    documentation AS ""Documentation"",
                    description AS ""Description"",
                    created_by_admin_id AS ""CreatedByAdminId"",
                    start_date AS ""StartDate"",
                    end_date AS ""EndDate"",
                    created_at AS ""CreatedAt"",
                    completed_at AS ""CompletedAt""
                FROM maintenance
                WHERE id = @Id";

            return await connection.QueryFirstOrDefaultAsync<Maintenance>(
                query,
                new { Id = id });
        }

        public async Task<IEnumerable<Maintenance>> GetRoomMaintenancesAsync(
            int roomId)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    room_id AS ""RoomId"",
                    maintenance_category AS ""MaintenanceCategory"",
                    priority_level AS ""PriorityLevel"",
                    facilities_serviced AS ""FacilitiesServiced"",
                    documentation AS ""Documentation"",
                    description AS ""Description"",
                    created_by_admin_id AS ""CreatedByAdminId"",
                    start_date AS ""StartDate"",
                    end_date AS ""EndDate"",
                    created_at AS ""CreatedAt"",
                    completed_at AS ""CompletedAt""
                FROM maintenance
                WHERE room_id = @RoomId
                ORDER BY created_at DESC";

            return await connection.QueryAsync<Maintenance>(
                query,
                new { RoomId = roomId });
        }

        public async Task<int> CreateMaintenanceAsync(Maintenance maintenance)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                INSERT INTO maintenance
                (
                    room_id,
                    maintenance_category,
                    priority_level,
                    facilities_serviced,
                    documentation,
                    description,
                    created_by_admin_id,
                    start_date,
                    end_date,
                    created_at
                )
                VALUES
                (
                    @RoomId,
                    @MaintenanceCategory,
                    @PriorityLevel,
                    @FacilitiesServiced,
                    @Documentation,
                    @Description,
                    @CreatedByAdminId,
                    @StartDate,
                    @EndDate,
                    NOW()
                )
                RETURNING id";

            return await connection.ExecuteScalarAsync<int>(
                query,
                maintenance);
        }

        public async Task<bool> CompleteMaintenanceAsync(int id)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                UPDATE maintenance
                SET completed_at = NOW()
                WHERE id = @Id
                  AND completed_at IS NULL";

            var result = await connection.ExecuteAsync(
                query,
                new { Id = id });

            return result > 0;
        }
    }
}