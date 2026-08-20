using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId);

        Task<Notification?> GetNotificationByIdAsync(int id);

        Task<bool> CreateNotificationAsync(Notification notification);

        Task<bool> MarkAsReadAsync(int notificationId, int userId);
    }
}