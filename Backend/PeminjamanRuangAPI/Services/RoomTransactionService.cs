using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Services
{
    public class RoomTransactionService
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly IRoomRepository _roomRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public RoomTransactionService(
            DatabaseConnection dbConnection,
            IRoomRepository roomRepository,
            IAuditLogRepository auditLogRepository)
        {
            _dbConnection = dbConnection;
            _roomRepository = roomRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<int> CreateRoomAsync(
            Room room,
            int adminId)
        {
            await using var connection = _dbConnection.CreateConnection();
            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                var roomId = await _roomRepository.CreateRoomAsync(
                    room,
                    connection,
                    transaction);

                if (roomId <= 0)
                {
                    await transaction.RollbackAsync();
                    return 0;
                }

                room.Id = roomId;

                var auditLog = new AuditLog
                {
                    AdminId = adminId,
                    Action = "CREATE",
                    EntityType = "ROOM",
                    EntityId = room.Id,
                    Changes =
                        $"Room '{room.Name}' dibuat. " +
                        $"Location: {room.Location}, " +
                        $"Capacity : {room.Capacity}."
                };

                await _auditLogRepository.CreateAuditLogAsync(
                    auditLog,
                    connection,
                    transaction);

                await transaction.CommitAsync();

                return roomId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateRoomAsync(
            Room room,
            int adminId,
            string oldName,
            string oldLocation,
            int oldCapacity)
        {
            await using var connection = _dbConnection.CreateConnection();
            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                var success = await _roomRepository.UpdateRoomAsync(
                    room,
                    connection,
                    transaction);

                if (!success)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                var auditLog = new AuditLog
                {
                    AdminId = adminId,
                    Action = "UPDATE",
                    EntityType = "ROOM",
                    EntityId = room.Id,
                    Changes =
                        $"Room diperbarui. " +
                        $"Name: '{oldName}' -> '{room.Name}', " +
                        $"Location: '{oldLocation}' -> '{room.Location}', " +
                        $"Capacity: {oldCapacity} -> {room.Capacity}."
                };

                await _auditLogRepository.CreateAuditLogAsync(
                    auditLog,
                    connection,
                    transaction);

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}