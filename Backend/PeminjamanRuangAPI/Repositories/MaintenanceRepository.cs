using System.Transactions;
using Dapper;
using Npgsql;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public class MaintenanceRepository : IMaintenanceRepository
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly IBookingRepository _bookingRepository;

        public MaintenanceRepository(
            DatabaseConnection dbConnection,
            IBookingRepository bookingRepository)
        {
            _dbConnection = dbConnection;
            _bookingRepository = bookingRepository;
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
        
            await connection.OpenAsync();
        
            await using var transaction =
                await connection.BeginTransactionAsync();
        
            try
            {
                var maintenanceId =
                    await CreateMaintenanceWithStatusAsync(
                        maintenance,
                        reason,
                        connection,
                        transaction);
        
                if (maintenanceId <= 0)
                {
                    await transaction.RollbackAsync();
                    return 0;
                }
        
                await transaction.CommitAsync();
        
                return maintenanceId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> CreateMaintenanceWithStatusAsync(
            Maintenance maintenance,
            string reason,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string lockRoomQuery = @"
                SELECT id
                FROM rooms
                WHERE id = @RoomId
                FOR UPDATE";
            
            var lockedRoomId =
                await connection.QueryFirstOrDefaultAsync<int?>(
                    lockRoomQuery,
                    new { RoomId = maintenance.RoomId },
                    transaction);
            
            if (!lockedRoomId.HasValue)
            {
                return 0;
            }

            var maintenanceConflict =
                await HasMaintenanceScheduleConflictAsync(
                    maintenance.RoomId,
                    maintenance.StartDate,
                    maintenance.EndDate,
                    connection,
                    transaction);
            
            if (maintenanceConflict)
            {
                return -1;
            }

            var bookingConflict =
                await _bookingRepository.HasBookingConflictInDateRangeAsync(
                    maintenance.RoomId,
                    maintenance.StartDate,
                    maintenance.EndDate,
                    connection,
                    transaction);
            
            if (bookingConflict)
            {
                return -2;
            }
            
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
                    new
                    {
                        MaintenanceId = maintenanceId
                    },
                    transaction);
        
                if (activated == 0)
                {
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
                    {
                        RoomId = maintenance.RoomId
                    },
                    transaction);
        
                if (roomUpdated == 0)
                {
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
                    return 0;
                }
            }
        
            return maintenanceId;
        }

        public async Task<bool> CompleteMaintenanceWithStatusAsync(
            int maintenanceId,
            int roomId,
            int adminId)
        {
            await using var connection =
                _dbConnection.CreateConnection();
        
            await connection.OpenAsync();
        
            await using var transaction =
                await connection.BeginTransactionAsync();
        
            try
            {
                var completed =
                    await CompleteMaintenanceWithStatusAsync(
                        maintenanceId,
                        roomId,
                        adminId,
                        connection,
                        transaction);
        
                if (!completed)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
        
                await transaction.CommitAsync();
        
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CompleteMaintenanceWithStatusAsync(
            int maintenanceId,
            int roomId,
            int adminId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {

            const string lockRoomQuery = @"
                SELECT id
                FROM rooms
                WHERE id = @RoomId
                FOR UPDATE";
            
            var lockedRoomId =
                await connection.QueryFirstOrDefaultAsync<int?>(
                    lockRoomQuery,
                    new { RoomId = roomId },
                    transaction);
            
            if (!lockedRoomId.HasValue)
            {
                return false;
            }

            const string completeMaintenanceQuery = @"
                UPDATE maintenance
                SET completed_at = NOW()
                WHERE id = @MaintenanceId
                  AND room_id = @RoomId
                  AND activated_at IS NOT NULL
                  AND completed_at IS NULL";
            
            var maintenanceUpdated =
                await connection.ExecuteAsync(
                    completeMaintenanceQuery,
                    new
                    {
                        MaintenanceId = maintenanceId,
                        RoomId = roomId
                    },
                    transaction);
            
            if (maintenanceUpdated == 0)
            {
                return false;
            }

            const string effectiveStatusQuery = @"
                SELECT CASE
                    WHEN EXISTS (
                        SELECT 1
                        FROM maintenance
                        WHERE room_id = @RoomId
                          AND id <> @MaintenanceId
                          AND activated_at IS NOT NULL
                          AND completed_at IS NULL
                    )
                    THEN 'MAINTENANCE'
            
                    WHEN EXISTS (
                        SELECT 1
                        FROM room_cleaning_session
                        WHERE room_id = @RoomId
                          AND is_completed = false
                    )
                    THEN 'CLEANING'
            
                    ELSE 'ACTIVE'
                END";
            
            var effectiveStatus =
                await connection.ExecuteScalarAsync<string>(
                    effectiveStatusQuery,
                    new
                    {
                        RoomId = roomId,
                        MaintenanceId = maintenanceId
                    },
                    transaction);
            
            const string updateRoomQuery = @"
                UPDATE rooms
                SET
                    is_active = @IsActive,
                    updated_at = NOW()
                WHERE id = @RoomId";
            
            var roomUpdated =
                await connection.ExecuteAsync(
                    updateRoomQuery,
                    new
                    {
                        RoomId = roomId,
                        IsActive = effectiveStatus == "ACTIVE"
                    },
                    transaction);
            
            if (roomUpdated == 0)
            {
                return false;
            }
            
            const string latestStatusQuery = @"
                SELECT status
                FROM room_status_history
                WHERE room_id = @RoomId
                ORDER BY created_at DESC, id DESC
                LIMIT 1";
            
            var latestStatus =
                await connection.QueryFirstOrDefaultAsync<string?>(
                    latestStatusQuery,
                    new { RoomId = roomId },
                    transaction);
            
            if (latestStatus != effectiveStatus)
            {
                var reason = effectiveStatus switch
                {
                    "ACTIVE" => "Maintenance selesai.",
                    "MAINTENANCE" => "Maintenance lain masih berlangsung.",
                    "CLEANING" => "Cleaning masih berlangsung.",
                    _ => null
                };
            
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
                        @Status,
                        @Reason,
                        @AdminId,
                        NOW()
                    )";
            
                var historyInserted =
                    await connection.ExecuteAsync(
                        insertHistoryQuery,
                        new
                        {
                            RoomId = roomId,
                            Status = effectiveStatus,
                            Reason = reason,
                            AdminId = adminId
                        },
                        transaction);
            
                if (historyInserted == 0)
                {
                    return false;
                }
            }
            return true;
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

        public async Task<bool> HasMaintenanceConflictAsync(
            int roomId,
            DateOnly bookingDate,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string query = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM maintenance
                    WHERE room_id = @RoomId
                      AND completed_at IS NULL
                      AND start_date <= @BookingDate
                      AND (
                            end_date IS NULL
                            OR end_date >= @BookingDate
                          )
                )";
        
            return await connection.ExecuteScalarAsync<bool>(
                query,
                new
                {
                    RoomId = roomId,
                    BookingDate = bookingDate
                },
                transaction);
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
                const string lockRoomQuery = @"
                    SELECT id
                    FROM rooms
                    WHERE id = @RoomId
                    FOR UPDATE";
                
                var lockedRoomId =
                    await connection.QueryFirstOrDefaultAsync<int?>(
                        lockRoomQuery,
                        new { RoomId = roomId },
                        transaction);
                
                if (!lockedRoomId.HasValue)
                {
                    transaction.Rollback();
                    return false;
                }

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

        public async Task<bool> HasMaintenanceScheduleConflictAsync(
            int roomId,
            DateOnly startDate,
            DateOnly? endDate,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
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
                },
                transaction);
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

                const string lockRoomQuery = @"
                    SELECT id
                    FROM rooms
                    WHERE id = @RoomId
                    FOR UPDATE";
                
                var lockedRoomId =
                    await connection.QueryFirstOrDefaultAsync<int?>(
                        lockRoomQuery,
                        new { RoomId = roomId },
                        transaction);
                
                if (!lockedRoomId.HasValue)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

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
                        
                const string effectiveStatusQuery = @"
                    SELECT CASE
                        WHEN EXISTS (
                            SELECT 1
                            FROM maintenance
                            WHERE room_id = @RoomId
                              AND id <> @MaintenanceId
                              AND activated_at IS NOT NULL
                              AND completed_at IS NULL
                        )
                        THEN 'MAINTENANCE'
                
                        WHEN EXISTS (
                            SELECT 1
                            FROM room_cleaning_session
                            WHERE room_id = @RoomId
                              AND is_completed = false
                        )
                        THEN 'CLEANING'
                
                        ELSE 'ACTIVE'
                    END";
                
                var effectiveStatus =
                    await connection.ExecuteScalarAsync<string>(
                        effectiveStatusQuery,
                        new
                        {
                            RoomId = roomId,
                            MaintenanceId = maintenanceId
                        },
                        transaction);
                
                const string updateRoomQuery = @"
                    UPDATE rooms
                    SET
                        is_active = @IsActive,
                        updated_at = NOW()
                    WHERE id = @RoomId";
                
                var roomUpdated =
                    await connection.ExecuteAsync(
                        updateRoomQuery,
                        new
                        {
                            RoomId = roomId,
                            IsActive = effectiveStatus == "ACTIVE"
                        },
                        transaction);
                
                if (roomUpdated == 0)
                {
                    transaction.Rollback();
                    return false;
                }
                
                const string latestStatusQuery = @"
                    SELECT status
                    FROM room_status_history
                    WHERE room_id = @RoomId
                    ORDER BY created_at DESC, id DESC
                    LIMIT 1";
                
                var latestStatus =
                    await connection.QueryFirstOrDefaultAsync<string?>(
                        latestStatusQuery,
                        new { RoomId = roomId },
                        transaction);
                
                if (latestStatus != effectiveStatus)
                {
                    var reason = effectiveStatus switch
                    {
                        "ACTIVE" => "Jadwal maintenance selesai.",
                        "MAINTENANCE" => "Maintenance lain masih berlangsung.",
                        "CLEANING" => "Cleaning masih berlangsung.",
                        _ => null
                    };
                
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
                            @Status,
                            @Reason,
                            NULL,
                            NOW()
                        )";
                
                    var historyInserted =
                        await connection.ExecuteAsync(
                            insertHistoryQuery,
                            new
                            {
                                RoomId = roomId,
                                Status = effectiveStatus,
                                Reason = reason
                            },
                            transaction);
                
                    if (historyInserted == 0)
                    {
                        transaction.Rollback();
                        return false;
                    }
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