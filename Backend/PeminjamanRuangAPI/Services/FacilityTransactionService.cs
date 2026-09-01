using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Services
{
    public class FacilityTransactionService
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly IFacilityRepository _facilityRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public FacilityTransactionService(
            DatabaseConnection dbConnection,
            IFacilityRepository facilityRepository,
            IAuditLogRepository auditLogRepository)
        {
            _dbConnection = dbConnection;
            _facilityRepository = facilityRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<int> CreateFacilityAsync(
            Facility facility,
            int adminId)
        {
            await using var connection = _dbConnection.CreateConnection();
            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                var facilityId =
                    await _facilityRepository.CreateFacilityAsync(
                        facility,
                        connection,
                        transaction);

                if (facilityId <= 0)
                {
                    await transaction.RollbackAsync();
                    return 0;
                }

                facility.Id = facilityId;

                var auditLog = new AuditLog
                {
                    AdminId = adminId,
                    Action = "CREATE",
                    EntityType = "FACILITY",
                    EntityId = facility.Id,
                    Changes = $"Facility'{facility.Name}' dibuat."
                };

                await _auditLogRepository.CreateAuditLogAsync(
                    auditLog,
                    connection,
                    transaction);

                await transaction.CommitAsync();

                return facilityId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateFacilityAsync(
            Facility facility,
            int adminId,
            string oldName,
            string? oldDescription)
        {
            await using var connection = _dbConnection.CreateConnection();
            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                var success =
                    await _facilityRepository.UpdateFacilityAsync(
                        facility,
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
                    EntityType = "FACILITY",
                    EntityId = facility.Id,
                    Changes =
                        $"Facility diperbarui. " +
                        $"Name: '{oldName}' -> '{facility.Name}', " +
                        $"Description: '{oldDescription}' -> '{facility.Description}'."
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

        public async Task<bool> DeleteFacilityAsync(
            Facility facility,
            int adminId)
        {
            await using var connection = _dbConnection.CreateConnection();
            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                var success =
                    await _facilityRepository.DeleteFacilityAsync(
                        facility.Id,
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
                    Action = "DELETE",
                    EntityType = "FACILITY",
                    EntityId = facility.Id,
                    Changes = $"Facility '{facility.Name}' dihapus."
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