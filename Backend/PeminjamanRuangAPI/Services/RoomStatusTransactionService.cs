using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Services
{
    public class RoomStatusTransactionService
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly IRoomStatusHistoryRepository _roomStatusRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public RoomStatusTransactionService(
            DatabaseConnection dbConnection,
            IRoomStatusHistoryRepository roomStatusRepository,
            IBookingRepository bookingRepository,
            IAuditLogRepository auditLogRepository)
        {
            _dbConnection = dbConnection;
            _roomStatusRepository = roomStatusRepository;
            _bookingRepository = bookingRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<int> ChangeRoomStatusAsync(
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
                // Semua operasi yang dapat mengubah availability room
                // harus serialize melalui row rooms yang sama.
                var roomLocked =
                    await _roomStatusRepository.LockRoomAsync(
                        roomId,
                        connection,
                        transaction);

                if (!roomLocked)
                {
                    await transaction.RollbackAsync();
                    return 0;
                }

                var latestStatus =
                    await _roomStatusRepository.GetLatestRoomStatusAsync(
                        roomId,
                        connection,
                        transaction);

                var latestStatusName =
                    latestStatus?.Status ?? "UNKNOWN";

                if (latestStatusName == status)
                {
                    await transaction.RollbackAsync();
                    return -1;
                }

                // Status lifecycle otomatis tidak boleh ditimpa manual.
                if (latestStatusName is "MAINTENANCE" or "CLEANING")
                {
                    await transaction.RollbackAsync();
                    return -2;
                }

                // CRITICAL:
                // pengecekan dilakukan SETELAH room lock diperoleh,
                // menggunakan connection + transaction yang sama.
                if (status == "OUT_OF_SERVICE")
                {
                    var currentlyInUse =
                        await _bookingRepository
                            .IsRoomCurrentlyInUseAsync(
                                roomId,
                                connection,
                                transaction);

                    if (currentlyInUse)
                    {
                        await transaction.RollbackAsync();
                        return -3;
                    }
                }

                var success =
                    await _roomStatusRepository.ChangeRoomStatusAsync(
                        roomId,
                        status,
                        reason,
                        adminId,
                        connection,
                        transaction);

                if (!success)
                {
                    await transaction.RollbackAsync();
                    return 0;
                }

                var auditAction =
                    status == "ACTIVE"
                        ? "ACTIVATE"
                        : "DEACTIVATE";

                var auditLog = new AuditLog
                {
                    AdminId = adminId,
                    Action = auditAction,
                    EntityType = "ROOM",
                    EntityId = roomId,
                    Changes =
                        $"Status room berubah dari {latestStatusName} menjadi {status}"
                };

                var auditCreated =
                    await _auditLogRepository.CreateAuditLogAsync(
                        auditLog,
                        connection,
                        transaction);

                if (!auditCreated)
                {
                    await transaction.RollbackAsync();
                    return 0;
                }

                await transaction.CommitAsync();

                return 1;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}