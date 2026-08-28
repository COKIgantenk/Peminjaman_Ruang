using Dapper;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public AuditLogRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<bool> CreateAuditLogAsync(AuditLog auditLog)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                INSERT INTO audit_log
                (
                    admin_id,
                    action,
                    entity_type,
                    entity_id,
                    changes,
                    created_at
                )
                VALUES
                (
                    @AdminId,
                    @Action,
                    @EntityType,
                    @EntityId,
                    @Changes,
                    NOW()
                )";

            var result =
                await connection.ExecuteAsync(query, auditLog);

            return result > 0;
        }

        public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync()
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    admin_id AS ""AdminId"",
                    action AS ""Action"",
                    entity_type AS ""EntityType"",
                    entity_id AS ""EntityId"",
                    changes AS ""Changes"",
                    created_at AS ""CreatedAt""
                FROM audit_log
                ORDER BY created_at DESC";

            return await connection.QueryAsync<AuditLog>(query);
        }

        public async Task<IEnumerable<AuditLog>>
            GetAuditLogsByEntityAsync(
                string entityType,
                int entityId)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    admin_id AS ""AdminId"",
                    action AS ""Action"",
                    entity_type AS ""EntityType"",
                    entity_id AS ""EntityId"",
                    changes AS ""Changes"",
                    created_at AS ""CreatedAt""
                FROM audit_log
                WHERE entity_type = @EntityType
                  AND entity_id = @EntityId
                ORDER BY created_at DESC";

            return await connection.QueryAsync<AuditLog>(
                query,
                new
                {
                    EntityType = entityType,
                    EntityId = entityId
                });
        }
    }
}