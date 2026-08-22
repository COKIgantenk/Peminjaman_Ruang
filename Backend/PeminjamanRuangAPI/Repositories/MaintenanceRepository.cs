using System.Transactions;
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

        public async Task<int> CreateMaintenanceWithStatusAsync(
            Maintenance maintenance,
            string reason)
        {
            using var connection = _dbConnection.CreateConnection();

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                const string insertMaintenanceQuery = @"
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

                var maintenanceId =
                    await connection.ExecuteScalarAsync<int>(
                        insertMaintenanceQuery,
                        maintenance,
                        transaction);

                if (maintenanceId <= 0)
                {
                    transaction.Rollback();
                    return 0;
                }

                const string updateRoomQuery = @"
                    UPDATE rooms
                    SET
                        is_active = false,
                        updated_at = NOW()
                    WHERE id = @RoomId";

                var roomUpdated = await connection.ExecuteAsync(
                    updateRoomQuery,
                    new
                    {
                        RoomId = maintenance.RoomId
                    },
                    transaction);

                if (roomUpdated == 0)
                {
                    transaction.Rollback();
                    return 0;
                }

                const string insertHistoryQuery = @"
                    INSERT INTO room_status_history
                    (
                        room_id,
                        status,
                        reason,
                        changed_by_admin_id,
                        created_at
                    )
                    VALUES
                    (
                        @RoomId,
                        'MAINTENANCE',
                        @Reason,
                        @AdminId,
                        NOW()
                    )";

                var historyInserted = await connection.ExecuteAsync(
                    insertHistoryQuery,
                    new
                    {
                        RoomId = maintenance.RoomId,
                        Reason = reason,
                        AdminId = maintenance.CreatedByAdminId
                    },
                    transaction);

                if (historyInserted == 0)
                {
                    transaction.Rollback();
                    return 0;
                }

                transaction.Commit();

                return maintenanceId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> CompleteMaintenanceWithStatusAsync(
            int maintenanceId,
            int roomId,
            int adminId)
        {
            using var connection = _dbConnection.CreateConnection();
        
            connection.Open();
        
            using var transaction = connection.BeginTransaction();
        
            try
            {
                const string completeMaintenanceQuery = @"
                    UPDATE maintenance
                    SET completed_at = NOW()
                    WHERE id = @MaintenanceId
                      AND completed_at IS NULL";
        
                var maintenanceUpdated = await connection.ExecuteAsync(
                    completeMaintenanceQuery,
                    new
                    {
                        MaintenanceId = maintenanceId
                    },
                    transaction);
        
                if (maintenanceUpdated == 0)
                {
                    transaction.Rollback();
                    return false;
                }
        
                const string updateRoomQuery = @"
                    UPDATE rooms
                    SET
                        is_active = true,
                        updated_at = NOW()
                    WHERE id = @RoomId";
        
                var roomUpdated = await connection.ExecuteAsync(
                    updateRoomQuery,
                    new
                    {
                        RoomId = roomId
                    },
                    transaction);
        
                if (roomUpdated == 0)
                {
                    transaction.Rollback();
                    return false;
                }
        
                const string insertHistoryQuery = @"
                    INSERT INTO room_status_history
                    (
                        room_id,
                        status,
                        reason,
                        changed_by_admin_id,
                        created_at
                    )
                    VALUES
                    (
                        @RoomId,
                        'ACTIVE',
                        'Maintenance selesai.',
                        @AdminId,
                        NOW()
                    )";
        
                var historyInserted = await connection.ExecuteAsync(
                    insertHistoryQuery,
                    new
                    {
                        RoomId = roomId,
                        AdminId = adminId
                    },
                    transaction);
        
                if (historyInserted == 0)
                {
                    transaction.Rollback();
                    return false;
                }
        
                transaction.Commit();
        
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }    
    }    
}    