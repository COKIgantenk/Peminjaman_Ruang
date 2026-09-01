using Dapper;
using Npgsql;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public class RoomCleaningSessionRepository : IRoomCleaningSessionRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public RoomCleaningSessionRepository(
            DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<RoomCleaningSession>>
            GetAllCleaningSessionsAsync()
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    room_id AS ""RoomId"",
                    booking_id AS ""BookingId"",
                    cleaning_duration AS ""CleaningDuration"",
                    custom_duration_minutes AS ""CustomDurationMinutes"",
                    start_time AS ""StartTime"",
                    scheduled_end_time AS ""ScheduledEndTime"",
                    end_time AS ""EndTime"",
                    is_completed AS ""IsCompleted"",
                    created_at AS ""CreatedAt""
                FROM room_cleaning_session
                ORDER BY created_at DESC";

            return await connection
                .QueryAsync<RoomCleaningSession>(query);
        }

        public async Task<RoomCleaningSession?>
            GetCleaningSessionByIdAsync(int id)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    room_id AS ""RoomId"",
                    booking_id AS ""BookingId"",
                    cleaning_duration AS ""CleaningDuration"",
                    custom_duration_minutes AS ""CustomDurationMinutes"",
                    start_time AS ""StartTime"",
                    scheduled_end_time AS ""ScheduledEndTime"",
                    end_time AS ""EndTime"",
                    is_completed AS ""IsCompleted"",
                    created_at AS ""CreatedAt""
                FROM room_cleaning_session
                WHERE id = @Id";

            return await connection
                .QueryFirstOrDefaultAsync<RoomCleaningSession>(
                    query,
                    new { Id = id });
        }

        public async Task<IEnumerable<RoomCleaningSession>>
            GetRoomCleaningSessionsAsync(int roomId)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    room_id AS ""RoomId"",
                    booking_id AS ""BookingId"",
                    cleaning_duration AS ""CleaningDuration"",
                    custom_duration_minutes AS ""CustomDurationMinutes"",
                    start_time AS ""StartTime"",
                    end_time AS ""EndTime"",
                    is_completed AS ""IsCompleted"",
                    created_at AS ""CreatedAt""
                FROM room_cleaning_session
                WHERE room_id = @RoomId
                ORDER BY created_at DESC";

            return await connection.QueryAsync<RoomCleaningSession>(
                query,
                new { RoomId = roomId });
        }

        public async Task<int> CreateAutomaticCleaningSessionAsync(
            int roomId,
            int bookingId)
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
                    return 0;
                }

                const string existingCleaningQuery = @"
                    SELECT EXISTS (
                        SELECT 1
                        FROM room_cleaning_session
                        WHERE booking_id = @BookingId
                    )";
                
                var cleaningAlreadyExists =
                    await connection.ExecuteScalarAsync<bool>(
                        existingCleaningQuery,
                        new { BookingId = bookingId },
                        transaction);
                
                if (cleaningAlreadyExists)
                {
                    transaction.Rollback();
                    return 0;
                }

                const string insertCleaningQuery = @"
                    INSERT INTO room_cleaning_session
                    (
                        room_id,
                        booking_id,
                        cleaning_duration,
                        custom_duration_minutes,
                        start_time,
                        end_time,
                        is_completed,
                        created_at
                    )
                    VALUES
                    (
                        @RoomId,
                        @BookingId,
                        NULL,
                        NULL,
                        NOW(),
                        NULL,
                        false,
                        NOW()
                    )
                    RETURNING id";

                var cleaningSessionId =
                    await connection.ExecuteScalarAsync<int>(
                        insertCleaningQuery,
                        new
                        {
                            RoomId = roomId,
                            BookingId = bookingId
                        },
                        transaction);

                if (cleaningSessionId <= 0)
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
                    new { RoomId = roomId },
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
                        'CLEANING',
                        NULL,
                        NULL,
                        NOW()
                        )";

                var historyInserted = await connection.ExecuteAsync(
                    insertHistoryQuery,
                    new { RoomId = roomId },
                    transaction);

                if (historyInserted == 0)
                {
                    transaction.Rollback();
                    return 0;
                }

                transaction.Commit();

                return cleaningSessionId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }    

        public async Task<bool> SetCleaningDurationAsync(
            int id,
            string cleaningDuration,
            int? customDurationMinutes)
        {
            await using var connection =
                _dbConnection.CreateConnection();
        
            await connection.OpenAsync();
        
            await using var transaction =
                await connection.BeginTransactionAsync();
        
            try
            {
                var success =
                    await SetCleaningDurationAsync(
                        id,
                        cleaningDuration,
                        customDurationMinutes,
                        connection,
                        transaction);
        
                if (!success)
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

        public async Task<bool> SetCleaningDurationAsync(
            int id,
            string cleaningDuration,
            int? customDurationMinutes,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string query = @"
                UPDATE room_cleaning_session
                SET
                    cleaning_duration = @CleaningDuration,
                    custom_duration_minutes = @CustomDurationMinutes,
                    scheduled_end_time =
                        CASE
                            WHEN @CleaningDuration = '10_MINUTES'
                                THEN NOW() + INTERVAL '10 minutes'
        
                            WHEN @CleaningDuration = '20_MINUTES'
                                THEN NOW() + INTERVAL '20 minutes'
        
                            WHEN @CleaningDuration = '30_MINUTES'
                                THEN NOW() + INTERVAL '30 minutes'
        
                            WHEN @CleaningDuration = 'CUSTOM'
                                THEN NOW() + (@CustomDurationMinutes * INTERVAL '1 minute')
        
                            ELSE NULL
                        END
                WHERE id = @Id
                  AND is_completed = false";
        
            var result =
                await connection.ExecuteAsync(
                    query,
                    new
                    {
                        Id = id,
                        CleaningDuration = cleaningDuration,
                        CustomDurationMinutes = customDurationMinutes
                    },
                    transaction);
        
            return result > 0;
        }

        public async Task<IEnumerable<RoomCleaningSession>>
            GetCleaningSessionsReadyToCompleteAsync()
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    room_id AS ""RoomId"",
                    booking_id AS ""BookingId"",
                    cleaning_duration AS ""CleaningDuration"",
                    custom_duration_minutes AS ""CustomDurationMinutes"",
                    start_time AS ""StartTime"",
                    scheduled_end_time AS ""ScheduledEndTime"",
                    end_time AS ""EndTime"",
                    is_completed AS ""IsCompleted"",
                    created_at AS ""CreatedAt""
                FROM room_cleaning_session
                WHERE is_completed = false 
                  AND scheduled_end_time IS NOT NULL
                  AND scheduled_end_time <= NOW()
                ORDER BY scheduled_end_time";

            return await connection.QueryAsync<RoomCleaningSession>(query);
        }

        public async Task<bool> CompleteAutomaticCleaningWithStatusAsync(
            int cleaningSessionId,
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

                const string completeCleaningQuery = @"
                    UPDATE room_cleaning_session
                    SET
                        is_completed = true,
                        end_time = NOW()
                    WHERE id = @CleaningSessionId
                      AND is_completed = false";

                var cleaningUpdated =
                    await connection.ExecuteAsync(
                        completeCleaningQuery,
                        new
                        {
                            CleaningSessionId = cleaningSessionId
                        },
                        transaction);

                if (cleaningUpdated == 0)
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
                              AND activated_at IS NOT NULL
                              AND completed_at IS NULL
                        )
                        THEN 'MAINTENANCE'
                
                        WHEN EXISTS (
                            SELECT 1
                            FROM room_cleaning_session
                            WHERE room_id = @RoomId
                              AND id <> @CleaningSessionId
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
                            CleaningSessionId = cleaningSessionId
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
                        "ACTIVE" => "Cleaning selesai otomatis.",
                        "MAINTENANCE" => "Maintenance masih berlangsung.",
                        "CLEANING" => "Cleaning lain masih berlangsung.",
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

        public async Task<RoomCleaningSession?> GetCleaningSessionByIdForUpdateAsync(
            int id,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string query = @"
                SELECT
                    id AS ""Id"",
                    room_id AS ""RoomId"",
                    booking_id AS ""BookingId"",
                    cleaning_duration AS ""CleaningDuration"",
                    custom_duration_minutes AS ""CustomDurationMinutes"",
                    start_time AS ""StartTime"",
                    scheduled_end_time AS ""ScheduledEndTime"",
                    end_time AS ""EndTime"",
                    is_completed AS ""IsCompleted"",
                    created_at AS ""CreatedAt""
                FROM room_cleaning_session
                WHERE id = @Id
                FOR UPDATE";
        
            return await connection
                .QueryFirstOrDefaultAsync<RoomCleaningSession>(
                    query,
                    new { Id = id },
                    transaction);
        }
    }
}