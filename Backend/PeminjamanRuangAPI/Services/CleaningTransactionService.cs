using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Services
{
    public class CleaningTransactionService
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly IRoomCleaningSessionRepository _cleaningRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public CleaningTransactionService(
            DatabaseConnection dbConnection,
            IRoomCleaningSessionRepository cleaningRepository,
            IAuditLogRepository auditLogRepository)
        {
            _dbConnection = dbConnection;
            _cleaningRepository = cleaningRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<bool> SetCleaningDurationAsync(
            int cleaningSessionId,
            int roomId,
            int adminId,
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
                var session =
                    await _cleaningRepository.GetCleaningSessionByIdForUpdateAsync(
                        cleaningSessionId,
                        connection,
                        transaction);
            
                if (session == null ||
                    session.RoomId != roomId ||
                    session.IsCompleted)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            
                if (session.CleaningDuration == cleaningDuration &&
                    session.CustomDurationMinutes == customDurationMinutes)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            
                var updated =
                    await _cleaningRepository.SetCleaningDurationAsync(
                        cleaningSessionId,
                        cleaningDuration,
                        customDurationMinutes,
                        connection,
                        transaction);
            
                if (!updated)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            
                var durationDetail =
                    cleaningDuration == "CUSTOM"
                        ? $"CUSTOM {customDurationMinutes} menit"
                        : cleaningDuration;
            
                var auditLog = new AuditLog
                {
                    AdminId = adminId,
                    Action = "UPDATE",
                    EntityType = "CLEANING",
                    EntityId = cleaningSessionId,
                    Changes =
                        $"Durasi cleaning Room {roomId} diubah menjadi {durationDetail}."
                };
            
                var auditCreated =
                    await _auditLogRepository.CreateAuditLogAsync(
                        auditLog,
                        connection,
                        transaction);
            
                if (!auditCreated)
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
    }
}