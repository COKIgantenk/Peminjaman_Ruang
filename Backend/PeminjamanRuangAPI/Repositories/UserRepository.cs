using Dapper;
using Npgsql;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public UserRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT
                        id AS ""Id"",
                        email AS ""Email"",
                        password_hash AS ""PasswordHash"",
                        full_name AS ""FullName"",
                        phone_number AS ""PhoneNumber"",
                        department_id AS ""DepartmentId"",
                        role AS ""Role"",
                        is_active AS ""IsActive"",
                        last_login AS ""LastLogin"",
                        created_at AS ""CreatedAt"",
                        updated_at AS ""UpdatedAt"",
                        deleted_at AS ""DeletedAt""
                    FROM users
                    WHERE deleted_at IS NULL
                    ORDER BY id";

                return await connection.QueryAsync<User>(query);
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT
                        id AS ""Id"",
                        email AS ""Email"",
                        password_hash AS ""PasswordHash"",
                        full_name AS ""FullName"",
                        phone_number AS ""PhoneNumber"",
                        department_id AS ""DepartmentId"",
                        role AS ""Role"",
                        is_active AS ""IsActive"",
                        last_login AS ""LastLogin"",
                        created_at AS ""CreatedAt"",
                        updated_at AS ""UpdatedAt"",
                        deleted_at AS ""DeletedAt""
                    FROM users
                    WHERE id = @Id
                     AND deleted_at IS NULL";

                return await connection.QueryFirstOrDefaultAsync<User>(
                    query,
                    new { Id = id });
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT
                        id AS ""Id"",
                        email AS ""Email"",
                        password_hash AS ""PasswordHash"",
                        full_name AS ""FullName"",
                        phone_number AS ""PhoneNumber"",
                        department_id AS ""DepartmentId"",
                        role AS ""Role"",
                        is_active AS ""IsActive"",
                        last_login AS ""LastLogin"",
                        created_at AS ""CreatedAt"",
                        updated_at AS ""UpdatedAt"",
                        deleted_at AS ""DeletedAt""
                    FROM users
                    WHERE email = @Email
                     AND deleted_at IS NULL";

                return await connection.QueryFirstOrDefaultAsync<User>(
                    query,
                    new { Email = email });
            }
        }

        public async Task<int> CreateUserAsync(User user)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    INSERT INTO users 
                    (
                        email, 
                        password_hash, 
                        full_name, 
                        phone_number, 
                        department_id, 
                        role, 
                        is_active, 
                        created_at, 
                        updated_at
                    )
                    VALUES 
                    (
                        @Email, 
                        @PasswordHash, 
                        @FullName, 
                        @PhoneNumber, 
                        @DepartmentId, 
                        @Role, 
                        @IsActive, 
                        NOW(), 
                        NOW()
                    )
                    RETURNING id";
                
                return await connection.ExecuteScalarAsync<int>(
                    query,
                    user);
            }
        }

        public async Task<int> CreateUserAsync(
            User user,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string query = @"
                INSERT INTO users
                (
                    email,
                    password_hash,
                    full_name,
                    phone_number,
                    department_id,
                    role,
                    is_active,
                    created_at,
                    updated_at
                )
                VALUES
                (
                    @Email,
                    @PasswordHash,
                    @FullName,
                    @PhoneNumber,
                    @DepartmentId,
                    @Role,
                    @IsActive,
                    NOW(),
                    NOW()
                )
                RETURNING id";
        
            return await connection.ExecuteScalarAsync<int>(
                query,
                user,
                transaction);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    UPDATE users 
                    SET full_name = @FullName, phone_number = @PhoneNumber, department_id = @DepartmentId, 
                        role = @Role, is_active = @IsActive, updated_at = NOW()
                    WHERE id = @Id AND deleted_at IS NULL";
                
                var result = await connection.ExecuteAsync(query, user);
                return result > 0;
            }
        }

        public async Task<bool> UpdateUserAsync(
            User user,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string query = @"
                UPDATE users
                SET
                    full_name = @FullName,
                    phone_number = @PhoneNumber,
                    department_id = @DepartmentId,
                    role = @Role,
                    is_active = @IsActive,
                    updated_at = NOW()
                WHERE id = @Id
                  AND deleted_at IS NULL";
        
            var result = await connection.ExecuteAsync(
                query,
                user,
                transaction);
        
            return result > 0;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                // Soft delete
                const string query = @"
                    UPDATE users 
                    SET 
                        deleted_at = NOW(), 
                        updated_at = NOW() 
                    WHERE id = @Id
                      AND deleted_at IS NULL";

                var result = await connection.ExecuteAsync(query, new { Id = id });
                return result > 0;
            }
        }

        public async Task<bool> DeleteUserAsync(
            int id,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string query = @"
                UPDATE users
                SET
                    deleted_at = NOW(),
                    updated_at = NOW()
                WHERE id = @Id
                  AND deleted_at IS NULL";
        
            var result = await connection.ExecuteAsync(
                query,
                new { Id = id },
                transaction);
        
            return result > 0;
        }

        public async Task<User?> GetDeletedUserByIdAsync(int id)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query =@"
                SELECT
                    id AS ""Id"",
                    email AS ""Email"",
                    password_hash AS ""PasswordHash"",
                    full_name AS ""FullName"",
                    phone_number AS ""PhoneNumber"",
                    department_id AS ""DepartmentId"",
                    role AS ""Role"",
                    is_active AS ""IsActive"",
                    last_login AS ""LastLogin"",
                    created_at AS ""CreatedAt"",
                    updated_at AS ""UpdatedAt"",
                    deleted_at AS ""DeletedAt""
                FROM users
                WHERE id = @Id
                  AND deleted_at IS NOT NULL";

            return await connection.QueryFirstOrDefaultAsync<User>(
                query,
                new { Id = id });
        }

        public async Task<bool> RestoreUserAsync(int id)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                UPDATE users
                SET
                    deleted_at = NULL,
                    updated_at = NOW()
                WHERE id = @Id
                  AND deleted_at IS NOT NULL";

            var result = await  connection.ExecuteAsync(
                query,
                new { Id = id});

            return result > 0;
        }

        public async Task<bool> RestoreUserAsync(
            int id,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string query = @"
                UPDATE users
                SET
                    deleted_at = NULL,
                    updated_at = NOW()
                WHERE id = @Id
                  AND deleted_at IS NOT NULL";
        
            var result = await connection.ExecuteAsync(
                query,
                new { Id = id },
                transaction);
        
            return result > 0;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = "SELECT COUNT(*) FROM users WHERE email = @Email AND deleted_at IS NULL";
                var count = await connection.ExecuteScalarAsync<int>(query, new { Email = email });
                return count > 0;
            }
        }

        public async Task<int> CountActiveAdminAsync()
        {
            using var connection = _dbConnection.CreateConnection();
            
            const string query = @"
                SELECT COUNT (*)
                FROM users
                WHERE role = 'ADMIN'
                  AND is_active = true
                  AND deleted_at IS NULL";

            return await connection.ExecuteScalarAsync<int>(query);
        }

        public async Task<int> CountActiveAdminAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string lockQuery = @"
                SELECT pg_advisory_xact_lock(82736491);
            ";
        
            await connection.ExecuteAsync(
                lockQuery,
                transaction: transaction);
        
            const string countQuery = @"
                SELECT COUNT(*)
                FROM users
                WHERE role = 'ADMIN'
                  AND is_active = TRUE
                  AND deleted_at IS NULL;
            ";
        
            return await connection.ExecuteScalarAsync<int>(
                countQuery,
                transaction: transaction);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    SELECT
                        id AS ""Id"",
                        email AS ""Email"",
                        password_hash AS ""PasswordHash"",
                        full_name AS ""FullName"",
                        phone_number AS ""PhoneNumber"",
                        department_id AS ""DepartmentId"",
                        role AS ""Role"",
                        is_active AS ""IsActive"",
                        last_login AS ""LastLogin"",
                        created_at AS ""CreatedAt"",
                        updated_at AS ""UpdatedAt"",
                        deleted_at AS ""DeletedAt""
                    FROM users
                    WHERE role = @Role
                     AND is_active = true
                     AND deleted_at IS NULL
                    ORDER BY id";

                return await connection.QueryAsync<User>(
                    query, 
                    new { Role = role });
            }
        }
    }
}