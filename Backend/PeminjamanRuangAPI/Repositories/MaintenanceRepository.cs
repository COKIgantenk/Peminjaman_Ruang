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
                    completed_at AS ""CompletedAt"",
                    activated_at AS ""ActivatedAt""
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
                    completed_at AS ""CompletedAt"",
                    activated_at AS ""ActivatedAt""
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
                    completed_at AS ""CompletedAt"",
                    activated_at AS ""ActivatedAt""
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

                var today = DateOnly.FromDateTime(DateTime.Today);

                if (maintenance.StartDate <= today)
                {
                    const string activateQuery = @"
                    UPDATE maintenance
                    SET activated_at = NOW()
                    WHERE id = @MaintenanceId
                       AND activated_at IS NULL";

                    var activated = await connection.ExecuteAsync(
                        activateQuery,
                        new { MaintenanceId = maintenanceId },
                        transaction);

                    if (activated == 0)
                    {
                        transaction.Rollback();
                        return 0;
                    }
    
                    const string roomQuery = @"
                        UPDATE rooms
                        SET
                            is_active = false,
                            updated_at = NOW()
                        WHERE id = @RoomId";
    
                    var roomUpdated = await connection.ExecuteAsync(
                        roomQuery,
                        new
                        { RoomId = maintenance.RoomId },
                        transaction);
    
                    if (roomUpdated == 0)
                    {
                        transaction.Rollback();
                        return 0;
                    }
    
                    const string historyQuery = @"
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
                        historyQuery,
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

        public async Task<bool> HasMaintenanceConflictAsync(
            int roomId,
            DateOnly bookingDate)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM maintenance
                    WHERE room_id = @RoomId
                       AND completed_at IS NULL
                       AND @BookingDate >= start_date
                       AND (
                            end_date IS NULL
                            OR @BookingDate <= end_date
                            )
                )";

            return await connection.ExecuteScalarAsync<bool>(
                query,
                new
                {
                    RoomId = roomId,
                    BookingDate = bookingDate
                });
        }

        public async Task<IEnumerable<Maintenance>> GetMaintenancesReadyToActivateAsync()
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
                    activated_at AS ""ActivatedAt"",
                    created_at AS ""CreatedAt"",
                    completed_at AS ""CompletedAt""
                FROM maintenance
                WHERE completed_at IS NULL
                  AND activated_at IS NULL
                  AND start_date <= CURRENT_DATE
                ORDER BY start_date";

            return await connection.QueryAsync<Maintenance>(query);
        }

        public async Task<bool> ActivateMaintenanceWithStatusAsync(
            int maintenanceId,
            int roomId,
            int adminId,
            string reason)
        {
            using var connection = _dbConnection.CreateConnection();
        
            connection.Open();
        
            using var transaction = connection.BeginTransaction();
        
            try
            {
                const string activateQuery = @"
                    UPDATE maintenance
                    SET activated_at = NOW()
                    WHERE id = @MaintenanceId
                      AND activated_at IS NULL
                      AND completed_at IS NULL";
        
                var activated = await connection.ExecuteAsync(
                    activateQuery,
                    new { MaintenanceId = maintenanceId },
                    transaction);
        
                if (activated == 0)
                {
                    transaction.Rollback();
                    return false;
                }
        
                const string roomQuery = @"
                    UPDATE rooms
                    SET
                        is_active = false,
                        updated_at = NOW()
                    WHERE id = @RoomId";
        
                var roomUpdated = await connection.ExecuteAsync(
                    roomQuery,
                    new { RoomId = roomId },
                    transaction);
        
                if (roomUpdated == 0)
                {
                    transaction.Rollback();
                    return false;
                }
        
                const string historyQuery = @"
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
        
                await connection.ExecuteAsync(
                    historyQuery,
                    new
                    {
                        RoomId = roomId,
                        Reason = reason,
                        AdminId = adminId
                    },
                    transaction);
        
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> HasMaintenanceScheduleConflictAsync(
            int roomId,
            DateOnly startDate,
            DateOnly? endDate)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM maintenance
                    WHERE room_id = @RoomId
                      AND completed_at IS NULL
                      AND (
                            @EndDate IS NULL
                            OR start_date <= @EndDate
                          )
                      AND (
                            end_date IS NULL
                            OR end_date >= @StartDate
                          )
                )";
            
            return await connection.ExecuteScalarAsync<bool>(
                query,
                new
                {
                    RoomId = roomId,
                    StartDate = startDate,
                    EndDate = endDate
                });
        } 

        public async Task<IEnumerable<Maintenance>>
            GetMaintenancesReadyToCompleteAsync()
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
                    activated_at AS ""ActivatedAt"",
                    created_at AS ""CreatedAt"",
                    completed_at AS ""CompletedAt""
                FROM maintenance
                WHERE activated_at IS NOT NULL
                  AND completed_at IS NULL
                  AND end_date IS NOT NULL
                  AND end_date < CURRENT_DATE
                ORDER BY end_date";
        
            return await connection.QueryAsync<Maintenance>(query);
        }   

        public async Task<bool> CompleteScheduledMaintenanceWithStatusAsync(
            int maintenanceId,
            int roomId)
        {
            using var connection = _dbConnection.CreateConnection();
        
            connection.Open();
        
            using var transaction = connection.BeginTransaction();
        
            try
            {
                const string completeQuery = @"
                    UPDATE maintenance
                    SET completed_at = NOW()
                    WHERE id = @MaintenanceId
                      AND activated_at IS NOT NULL
                      AND completed_at IS NULL";
        
                var completed = await connection.ExecuteAsync(
                    completeQuery,
                    new { MaintenanceId = maintenanceId },
                    transaction);
        
                if (completed == 0)
                {
                    transaction.Rollback();
                    return false;
                }
        
                const string roomQuery = @"
                    UPDATE rooms
                    SET
                        is_active = true,
                        updated_at = NOW()
                    WHERE id = @RoomId";
        
                var roomUpdated = await connection.ExecuteAsync(
                    roomQuery,
                    new { RoomId = roomId },
                    transaction);
        
                if (roomUpdated == 0)
                {
                    transaction.Rollback();
                    return false;
                }
        
                const string historyQuery = @"
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
                        'Jadwal maintenance selesai.',
                        NULL,
                        NOW()
                    )";
        
                var historyInserted = await connection.ExecuteAsync(
                    historyQuery,
                    new { RoomId = roomId },
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