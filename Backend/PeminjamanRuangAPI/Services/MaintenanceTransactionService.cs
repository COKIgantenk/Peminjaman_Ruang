using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Services
{
    public class MaintenanceTransactionService
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IBookingRepository _bookingRepository;

        public MaintenanceTransactionService(
            DatabaseConnection dbConnection,
            IMaintenanceRepository maintenanceRepository,
            IAuditLogRepository auditLogRepository,
            IBookingRepository bookingRepository)
        {
            _dbConnection = dbConnection;
            _maintenanceRepository = maintenanceRepository;
            _auditLogRepository = auditLogRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<int> CreateMaintenanceAsync(
            Maintenance maintenance,
            string reason)
        {
            await using var connection =
                _dbConnection.CreateConnection();

            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                var maintenanceId =
                    await _maintenanceRepository
                        .CreateMaintenanceWithStatusAsync(
                            maintenance,
                            reason,
                            connection,
                            transaction);

                if (maintenanceId == -1)
                {
                    await transaction.RollbackAsync();
                    return -1;
                }

                if (maintenanceId == -2)
                {
                    await transaction.RollbackAsync();
                    return -2;
                }
                
                if (maintenanceId <= 0)
                {
                    await transaction.RollbackAsync();
                    return 0;
                }

                var auditLog = new AuditLog
                {
                    AdminId = maintenance.CreatedByAdminId,
                    Action = "CREATE",
                    EntityType = "MAINTENANCE",
                    EntityId = maintenanceId,
                    Changes =
                        $"Maintenance dibuat untuk Room {maintenance.RoomId}, " +
                        $"tanggal {maintenance.StartDate}" +
                        $"{(maintenance.EndDate.HasValue
                            ? $" sampai {maintenance.EndDate.Value}"
                            : "")}. " +
                        $"Priority: {maintenance.PriorityLevel}."
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

                return maintenanceId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        public async Task<bool> CompleteMaintenanceAsync(
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
                    await _maintenanceRepository
                        .CompleteMaintenanceWithStatusAsync(
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
        
                var auditLog = new AuditLog
                {
                    AdminId = adminId,
                    Action = "COMPLETE",
                    EntityType = "MAINTENANCE",
                    EntityId = maintenanceId,
                    Changes =
                        $"Maintenance Room {roomId} diselesaikan."
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