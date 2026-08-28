using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Services
{
    public class AuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(
            IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task LogAsync(
            int adminId,
            string action,
            string entityType,
            int entityId,
            string? changes = null)
        {
            var auditLog = new AuditLog
            {
                AdminId = adminId,
                Action = action.Trim().ToUpperInvariant(),
                EntityType = entityType.Trim().ToUpperInvariant(),
                EntityId = entityId,
                Changes = changes
            };

            await _auditLogRepository
                .CreateAuditLogAsync(auditLog);
        }
    }
}