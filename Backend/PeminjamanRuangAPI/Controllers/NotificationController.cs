using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeminjamanRuangAPI.DTOs;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(
            INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<NotificationResponseDto>>>
            GetMyNotifications()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Token user tidak valid."
                });
            }

            var notifications =
                await _notificationRepository
                    .GetUserNotificationsAsync(userId);

            var response = notifications.Select(notification =>
                new NotificationResponseDto
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    BookingId = notification.BookingId,
                    NotificationType = notification.NotificationType,
                    EmailSent = notification.EmailSent,
                    SentAt = notification.SentAt,
                    CreatedAt = notification.CreatedAt
                });

            return Ok(response);
        }
    }
}