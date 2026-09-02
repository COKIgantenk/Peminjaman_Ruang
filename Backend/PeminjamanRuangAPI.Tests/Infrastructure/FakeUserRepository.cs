using Npgsql;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Tests.Infrastructure
{
    public sealed class FakeUserRepository : IUserRepository
    {
        private User? _currentUser;

        public void SetUser(User? user)
        {
            _currentUser = user;
        }

        public Task<User?> GetUserByIdAsync(int id)
        {
            if (_currentUser == null ||
                _currentUser.Id != id)
            {
                return Task.FromResult<User?>(null);
            }

            return Task.FromResult<User?>(_currentUser);
        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            if (_currentUser == null ||
                !string.Equals(
                    _currentUser.Email,
                    email,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<User?>(null);
            }

            return Task.FromResult<User?>(_currentUser);
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            IEnumerable<User> users =
                _currentUser == null
                    ? Array.Empty<User>()
                    : new[] { _currentUser };

            return Task.FromResult(users);
        }

        public Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            IEnumerable<User> users =
                _currentUser != null &&
                string.Equals(
                    _currentUser.Role,
                    role,
                    StringComparison.OrdinalIgnoreCase)
                    ? new[] { _currentUser }
                    : Array.Empty<User>();

            return Task.FromResult(users);
        }

        public Task<int> CreateUserAsync(User user) =>
            Task.FromResult(user.Id);

        public Task<int> CreateUserAsync(
            User user,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction) =>
            Task.FromResult(user.Id);

        public Task<bool> UpdateUserAsync(User user) =>
            Task.FromResult(true);

        public Task<bool> UpdateUserAsync(
            User user,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction) =>
            Task.FromResult(true);

        public Task<bool> DeleteUserAsync(int id) =>
            Task.FromResult(true);

        public Task<bool> DeleteUserAsync(
            int id,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction) =>
            Task.FromResult(true);

        public Task<User?> GetDeletedUserByIdAsync(int id) =>
            Task.FromResult<User?>(null);

        public Task<bool> RestoreUserAsync(int id) =>
            Task.FromResult(true);

        public Task<bool> RestoreUserAsync(
            int id,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction) =>
            Task.FromResult(true);

        public Task<bool> UserExistsAsync(string email) =>
            Task.FromResult(false);

        public Task<int> CountActiveAdminAsync() =>
            Task.FromResult(1);

        public Task<int> CountActiveAdminAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction) =>
            Task.FromResult(1);
    }
}