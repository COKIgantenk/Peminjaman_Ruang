using Npgsql;
using Dapper;
using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public NotificationRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    user_id AS ""UserId"",
                    booking_id AS ""BookingId"",
                    notification_type AS ""NotificationType"",
                    email_sent AS ""EmailSent"",
                    sent_at AS ""SentAt"",
                    is_read AS ""IsRead"",
                    read_at AS ""ReadAt"",
                    created_at AS ""CreatedAt""
                FROM notifications
                WHERE user_id = @UserId
                ORDER BY created_at DESC";

            return await connection.QueryAsync<Notification>(
                query,
                new { UserId = userId });
        }

        public async Task<Notification?> GetNotificationByIdAsync(int id)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                SELECT
                    id AS ""Id"",
                    user_id AS ""UserId"",
                    booking_id AS ""BookingId"",
                    notification_type AS ""NotificationType"",
                    email_sent AS ""EmailSent"",
                    sent_at AS ""SentAt"",
                    is_read AS ""IsRead"",
                    read_at AS ""ReadAt"",
                    created_at AS ""CreatedAt""
                FROM notifications
                WHERE id = @Id";

            return await connection.QueryFirstOrDefaultAsync<Notification>(
                query,
                new { Id = id });
        }

        public async Task<bool> CreateNotificationAsync(Notification notification)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                INSERT INTO notifications
                (
                    user_id,
                    booking_id,
                    notification_type,
                    email_sent,
                    sent_at,
                    created_at
                )
                VALUES
                (
                    @UserId,
                    @BookingId,
                    @NotificationType,
                    @EmailSent,
                    @SentAt,
                    NOW()
                )";

            var result = await connection.ExecuteAsync(
                query,
                notification);

            return result > 0;
        }

        public async Task<bool> CreateNotificationAsync(
            Notification notification,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            const string query = @"
                INSERT INTO notifications
                (
                    user_id,
                    booking_id,
                    notification_type,
                    email_sent,
                    sent_at,
                    created_at
                )
                VALUES
                (
                    @UserId,
                    @BookingId,
                    @NotificationType,
                    @EmailSent,
                    @SentAt,
                    NOW()
                )";
        
            var result = await connection.ExecuteAsync(
                query,
                notification,
                transaction);
        
            return result > 0;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            using var connection = _dbConnection.CreateConnection();

            const string query = @"
                UPDATE notifications
                SET 
                    is_read = TRUE,
                    read_at = NOW()
                WHERE id = @NotificationId 
                  AND user_id = @UserId";

            var result = await connection.ExecuteAsync(
                query,
                new 
                { 
                    NotificationId = notificationId, 
                    UserId = userId 
                });

            return result > 0;
        }
    }
}