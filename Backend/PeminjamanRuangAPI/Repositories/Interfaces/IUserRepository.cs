using Npgsql;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<int> CreateUserAsync(User user);
        Task<int> CreateUserAsync(
            User user,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> UpdateUserAsync(
            User user,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> DeleteUserAsync(
            int id,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<User?> GetDeletedUserByIdAsync(int id);
        Task<bool> RestoreUserAsync(int id);
        Task<bool> RestoreUserAsync(
            int id,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool> UserExistsAsync(string email);
        Task<int> CountActiveAdminAsync();
        Task<int> CountActiveAdminAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);

    }
}