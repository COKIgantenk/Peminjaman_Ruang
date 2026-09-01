using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Services
{
    public class DepartmentTransactionService
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public DepartmentTransactionService(
            DatabaseConnection dbConnection,
            IDepartmentRepository departmentRepository,
            IAuditLogRepository auditLogRepository)
        {
            _dbConnection = dbConnection;
            _departmentRepository = departmentRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<int> CreateDepartmentAsync(
            Department department,
            int adminId)
        {
            await using var connection = _dbConnection.CreateConnection();
            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                var departmentId =
                    await _departmentRepository.CreateDepartmentAsync(
                        department,
                        connection,
                        transaction);

                if (departmentId <= 0)
                {
                    await transaction.RollbackAsync();
                    return 0;
                }

                department.Id = departmentId;

                var auditLog = new AuditLog
                {
                    AdminId = adminId,
                    Action = "CREATE",
                    EntityType = "DEPARTMENT",
                    EntityId = departmentId,
                    Changes = $"Departemen '{department.Name}' dibuat."
                };
                
                var auditSuccess =
                    await _auditLogRepository.CreateAuditLogAsync(
                        auditLog,
                        connection,
                        transaction);
                
                if (!auditSuccess)
                {
                    await transaction.RollbackAsync();
                    return 0;
                }

                await transaction.CommitAsync();

                return departmentId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}