using Dapper;
using Npgsql;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public class BookingCancellationRepository
        : IBookingCancellationRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public BookingCancellationRepository(
            DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<BookingCancellation?> GetByBookingIdAsync(
            int bookingId)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    booking_id AS ""BookingId"",
                    cancellation_reason AS ""CancellationReason"",
                    cancelled_by_user_id AS ""CancelledByUserId"",
                    cancelled_at AS ""CancelledAt""
                FROM booking_cancellation
                WHERE booking_id = @BookingId";

            return await connection
                .QueryFirstOrDefaultAsync<BookingCancellation>(
                    query,
                    new { BookingId = bookingId });
        }

        public async Task<bool> CreateCancellationAsync(
            BookingCancellation cancellation)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                INSERT INTO booking_cancellation
                (
                    booking_id,
                    cancellation_reason,
                    cancelled_by_user_id,
                    cancelled_at
                )
                VALUES
                (
                    @BookingId,
                    @CancellationReason,
                    @CancelledByUserId,
                    NOW()
                )";

            var result = await connection.ExecuteAsync(
                query,
                cancellation);

            return result > 0;
        }

        public async Task<bool> CreateCancellationAsync(
            BookingCancellation cancellation,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string query = @"
                INSERT INTO booking_cancellation
                (
                    booking_id,
                    cancellation_reason,
                    cancelled_by_user_id,
                    cancelled_at
                )
                VALUES
                (
                    @BookingId,
                    @CancellationReason,
                    @CancelledByUserId,
                    NOW()
                )";
        
            var result = await connection.ExecuteAsync(
                query,
                new
                {
                    cancellation.BookingId,
                    cancellation.CancellationReason,
                    cancellation.CancelledByUserId
                },
                transaction);
        
            return result > 0;
        }
    }
}