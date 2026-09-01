using Npgsql;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IAuditLogRepository
    {
        Task<bool> CreateAuditLogAsync(AuditLog auditLog);
        Task<bool> CreateAuditLogAsync(
            AuditLog auditLog,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);

        Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync();

        Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(
            string entityType,
            int entityId);
    }
}