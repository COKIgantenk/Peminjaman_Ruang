using Npgsql;
using System.Data;

namespace PeminjamanRuangAPI.Data
{
    /// <summary>
    /// Database Connection Handler untuk PostgreSQL & Dapper
    /// </summary>
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /// <summary>
        /// Buka koneksi ke database
        /// </summary>
        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}