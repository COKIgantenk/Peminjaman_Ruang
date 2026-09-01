using Npgsql;

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
            _connectionString = 
                configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found in configuration.");
               
        }

        /// <summary>
        /// Membuat koneksi PostgreSQL
        /// </summary>
        public NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}