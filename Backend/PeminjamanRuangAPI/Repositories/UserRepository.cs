using Dapper;
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
                const string query = "SELECT * FROM users WHERE deleted_at IS NULL ORDER BY id";
                return await connection.QueryAsync<User>(query);
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = "SELECT * FROM users WHERE id = @Id AND deleted_at IS NULL";
                return await connection.QueryFirstOrDefaultAsync<User>(query, new { Id = id });
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = "SELECT * FROM users WHERE email = @Email AND deleted_at IS NULL";
                return await connection.QueryFirstOrDefaultAsync<User>(query, new { Email = email });
            }
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                const string query = @"
                    INSERT INTO users (email, password_hash, full_name, phone_number, department_id, role, is_active, created_at, updated_at)
                    VALUES (@Email, @PasswordHash, @FullName, @PhoneNumber, @DepartmentId, @Role, @IsActive, NOW(), NOW())";
                
                var result = await connection.ExecuteAsync(query, user);
                return result > 0;
            }
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

        public async Task<bool> DeleteUserAsync(int id)
        {
            using (var connection = _dbConnection.CreateConnection())
            {
                // Soft delete
                const string query = "UPDATE users SET deleted_at = NOW(), updated_at = NOW() WHERE id = @Id";
                var result = await connection.ExecuteAsync(query, new { Id = id });
                return result > 0;
            }
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
    }
}