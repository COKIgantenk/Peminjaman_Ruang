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
                    IsRead = notification.IsRead,
                    ReadAt = notification.ReadAt,
                    CreatedAt = notification.CreatedAt
                });

            return Ok(response);
        }

        [HttpPut("{id}/read")]
        public async Task<ActionResult> MarkAsRead(int id)
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

            var notification =
                await _notificationRepository
                    .GetNotificationByIdAsync(id);

            if (notification == null)
            {
                return NotFound(new
                {
                    message = "Notifikasi tidak ditemukan."
                });
            }

            if (notification.UserId != userId)
            {
                return Forbid();    
            }

            if (notification.IsRead)
            {
                return BadRequest(new
                {
                    message = "Notifikasi sudah dibaca sebelumnya."
                });
            }

            var success =
                await _notificationRepository
                    .MarkAsReadAsync(id, userId);

            if (!success)
            {
                return BadRequest(new
                {
                    message = "Gagal menandai notifikasi sebagai sudah dibaca."
                });
            }

            return Ok(new
            {
                message = "Notifikasi berhasil ditandai sebagai sudah dibaca."
            });
        }
    }
}