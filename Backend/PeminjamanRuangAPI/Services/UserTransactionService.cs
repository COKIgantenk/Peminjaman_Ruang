using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Services
{
    public class UserTransactionService
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public UserTransactionService(
            DatabaseConnection dbConnection,
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository)
        {
            _dbConnection = dbConnection;
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<int> CreateUserAsync(
            User user,
            int adminId)
        {
            await using var connection = _dbConnection.CreateConnection();
            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                var userId = await _userRepository.CreateUserAsync(
                    user,
                    connection,
                    transaction);

                if (userId <= 0)
                {
                    await transaction.RollbackAsync();
                    return 0;
                }

                user.Id = userId;

                var auditLog = new AuditLog
                {
                    AdminId = adminId,
                    Action = "CREATE",
                    EntityType = "USER",
                    EntityId = userId,
                    Changes =
                        $"User '{user.Email}' dengan role '{user.Role}' dibuat."
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

                return userId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(
            User user,
            int adminId,
            string oldFullName,
            string oldPhoneNumber,
            int oldDepartmentId,
            string oldRole,
            bool oldIsActive)
        {
            await using var connection = _dbConnection.CreateConnection();
            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                var removesAdminAccess =
                    oldRole == "ADMIN" &&
                    oldIsActive &&
                    (user.Role != "ADMIN" || !user.IsActive);
                
                if (removesAdminAccess)
                {
                    var activeAdminCount =
                        await _userRepository.CountActiveAdminAsync(
                            connection,
                            transaction);
                
                    if (activeAdminCount <= 1)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }

                var success = await _userRepository.UpdateUserAsync(
                    user,
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
                    EntityType = "USER",
                    EntityId = user.Id,
                    Changes =
                        $"User '{user.Email}' diperbarui. " +
                        $"FullName: '{oldFullName}' -> '{user.FullName}', " +
                        $"PhoneNumber: '{oldPhoneNumber}' -> '{user.PhoneNumber}', " +
                        $"DepartmentId: {oldDepartmentId} -> {user.DepartmentId}, " +
                        $"Role: '{oldRole}' -> '{user.Role}', " +
                        $"IsActive: {oldIsActive} -> {user.IsActive}."
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

        public async Task<bool> DeleteUserAsync(
            User user,
            int adminId)
        {
            await using var connection = _dbConnection.CreateConnection();
            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();


            try
            {
                if (user.Role == "ADMIN" && user.IsActive)
                {
                    var activeAdminCount =
                        await _userRepository.CountActiveAdminAsync(
                            connection,
                            transaction);
                
                    if (activeAdminCount <= 1)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
                
                var success = await _userRepository.DeleteUserAsync(
                    user.Id,
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
                    EntityType = "USER",
                    EntityId = user.Id,
                    Changes =
                        $"User '{user.Email}' dengan nama '{user.FullName}' dihapus."
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

        public async Task<bool> RestoreUserAsync(
            User deletedUser,
            int adminId)
        {
            await using var connection = _dbConnection.CreateConnection();
            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                var success = await _userRepository.RestoreUserAsync(
                    deletedUser.Id,
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
                    Action = "RESTORE",
                    EntityType = "USER",
                    EntityId = deletedUser.Id,
                    Changes =
                        $"User '{deletedUser.Email}' dengan nama '{deletedUser.FullName}' dipulihkan."
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