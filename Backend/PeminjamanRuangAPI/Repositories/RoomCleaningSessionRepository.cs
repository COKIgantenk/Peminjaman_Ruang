using Dapper;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public class RoomCleaningSessionRepository
        : IRoomCleaningSessionRepository
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

        public async Task<bool> CompleteCleaningWithStatusAsync(
            int cleaningSessionId,
            int roomId,
            int adminId)
        {
            using var connection = _dbConnection.CreateConnection();

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                const string completeCleaningQuery = @"
                    UPDATE room_cleaning_session
                    SET
                        is_completed = true,
                        end_time = NOW()
                    WHERE id = @CleaningSessionId
                      AND is_completed = false";

                var cleaningUpdated = await connection.ExecuteAsync(
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
                        NULL,
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

        public async Task<bool> SetCleaningDurationAsync(
            int cleaningSessionId,
            string cleaningDuration,
            int? customDurationMinutes)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                UPDATE room_cleaning_session
                SET
                    cleaning_duration = @CleaningDuration,
                    custom_duration_minutes = @CustomDurationMinutes
                WHERE id = @CleaningSessionId
                  AND is_completed = false";

            var result = await connection.ExecuteAsync(
                query,
                new
                {
                    CleaningSessionId = cleaningSessionId,
                    CleaningDuration = cleaningDuration,
                    CustomDurationMinutes = customDurationMinutes
                });

            return result > 0;
        }
    }
}