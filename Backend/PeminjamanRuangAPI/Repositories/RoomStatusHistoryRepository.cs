using Dapper;
using Npgsql;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public class RoomStatusHistoryRepository : IRoomStatusHistoryRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public RoomStatusHistoryRepository(
            DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<RoomStatusHistory>>
            GetRoomStatusHistoryAsync(int roomId)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    room_id AS ""RoomId"",
                    status AS ""Status"",
                    reason AS ""Reason"",
                    changed_by_admin_id AS ""ChangedByAdminId"",
                    created_at AS ""CreatedAt""
                FROM room_status_history
                WHERE room_id = @RoomId
                ORDER BY created_at DESC";

            return await connection.QueryAsync<RoomStatusHistory>(
                query,
                new { RoomId = roomId });
        }

        public async Task<RoomStatusHistory?>
            GetLatestRoomStatusAsync(int roomId)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    room_id AS ""RoomId"",
                    status AS ""Status"",
                    reason AS ""Reason"",
                    changed_by_admin_id AS ""ChangedByAdminId"",
                    created_at AS ""CreatedAt""
                FROM room_status_history
                WHERE room_id = @RoomId
                ORDER BY created_at DESC
                LIMIT 1";

            return await connection
                .QueryFirstOrDefaultAsync<RoomStatusHistory>(
                    query,
                    new { RoomId = roomId });
        }

        public async Task<RoomStatusHistory?>
            GetLatestRoomStatusAsync(
                int roomId,
                NpgsqlConnection connection,
                NpgsqlTransaction transaction)
        {
            const string query = @"
                SELECT
                    id AS ""Id"",
                    room_id AS ""RoomId"",
                    status AS ""Status"",
                    reason AS ""Reason"",
                    changed_by_admin_id AS ""ChangedByAdminId"",
                    created_at AS ""CreatedAt""
                FROM room_status_history
                WHERE room_id = @RoomId
                ORDER BY created_at DESC
                LIMIT 1";
        
            return await connection
                .QueryFirstOrDefaultAsync<RoomStatusHistory>(
                    query,
                    new
                    {
                        RoomId = roomId
                    },
                    transaction);
        }

        public async Task<bool> LockRoomAsync(
            int roomId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string query = @"
                SELECT id
                FROM rooms
                WHERE id = @RoomId
                FOR UPDATE";
        
            var lockedRoomId =
                await connection.QueryFirstOrDefaultAsync<int?>(
                    query,
                    new
                    {
                        RoomId = roomId
                    },
                    transaction);
        
            return lockedRoomId.HasValue;
        }

        public async Task<bool> CreateRoomStatusHistoryAsync(
            RoomStatusHistory roomStatusHistory)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
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
                    @ChangedByAdminId,
                    NOW()
                )";

            var result = await connection.ExecuteAsync(
                query,
                roomStatusHistory);

            return result > 0;
        }

        public async Task<bool> CreateRoomStatusHistoryAsync(
            RoomStatusHistory roomStatusHistory,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string query = @"
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
                    @ChangedByAdminId,
                    NOW()
                )";
        
            var result =
                await connection.ExecuteAsync(
                    query,
                    roomStatusHistory,
                    transaction);
        
            return result > 0;
        }

        public async Task<bool> ChangeRoomStatusAsync(
            int roomId,
            string status,
            string? reason,
            int adminId)
        {
            await using var connection =
                _dbConnection.CreateConnection();
        
            await connection.OpenAsync();
        
            await using var transaction =
                await connection.BeginTransactionAsync();
        
            try
            {
                var success =
                    await ChangeRoomStatusAsync(
                        roomId,
                        status,
                        reason,
                        adminId,
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

        public async Task<bool> ChangeRoomStatusAsync(
            int roomId,
            string status,
            string? reason,
            int adminId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            var isActive = status == "ACTIVE";
        
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
                        IsActive = isActive
                    },
                    transaction);
        
            if (roomUpdated == 0)
            {
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
                    @Status,
                    @Reason,
                    @ChangedByAdminId,
                    NOW()
                )";
        
            var historyInserted =
                await connection.ExecuteAsync(
                    insertHistoryQuery,
                    new
                    {
                        RoomId = roomId,
                        Status = status,
                        Reason = reason,
                        ChangedByAdminId = adminId
                    },
                    transaction);
        
            if (historyInserted == 0)
            {
                return false;
            }
        
            return true;
        }
    }
}