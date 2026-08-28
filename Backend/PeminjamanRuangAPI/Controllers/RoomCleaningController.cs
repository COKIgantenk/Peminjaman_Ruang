using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeminjamanRuangAPI.Services;
using PeminjamanRuangAPI.DTOs;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class RoomCleaningController : ControllerBase
    {
        private readonly IRoomCleaningSessionRepository _cleaningRepository;
        private readonly AuditLogService _auditLogService;
        
        public RoomCleaningController(
            IRoomCleaningSessionRepository cleaningRepository,
            AuditLogService auditLogService)
        {
            _cleaningRepository = cleaningRepository;
            _auditLogService = auditLogService;

        }

        
        [HttpPut("{id}/complete")]
        public async Task<ActionResult> CompleteCleaning(int id)
        {
            var session =
                await _cleaningRepository.GetCleaningSessionByIdAsync(id);

            if (session == null)
            {
                return NotFound(new
                {
                    message = "Cleaning session tidak ditemukan."
                });
            }

            if (session.IsCompleted)
            {
                return BadRequest(new
                {
                    message = "Cleaning session sudah selesai sebelumnya."
                });
            }

            var adminIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token admin tidak valid."
                });
            }

            var completed =
                await _cleaningRepository.CompleteCleaningWithStatusAsync(
                    id,
                    session.RoomId,
                    adminId);

            if (!completed)
            {
                return BadRequest(new
                {
                    message = "Cleaning session gagal diselesaikan."
                });
            }

            await _auditLogService.LogAsync(
                adminId,
                "COMPLETE",
                "CLEANING",
                session.Id,
                $"Cleaning Room {session.RoomId} diselesaikan dan Room kembali ACTIVE.");

            return Ok(new
            {
                message = "Cleaning session selesai dan room kembali aktif."
            });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomCleaningSessionResponseDto>>>
            GetAllCleaningSessions()
        {
            var sessions =
                await _cleaningRepository.GetAllCleaningSessionsAsync();

            var response = sessions.Select(session =>
                new RoomCleaningSessionResponseDto
                {
                    Id = session.Id,
                    RoomId = session.RoomId,
                    BookingId = session.BookingId,
                    CleaningDuration = session.CleaningDuration,
                    CustomDurationMinutes = session.CustomDurationMinutes,
                    StartTime = session.StartTime,
                    ScheduledEndTime = session.ScheduledEndTime,
                    EndTime = session.EndTime,
                    IsCompleted = session.IsCompleted,
                    CreatedAt = session.CreatedAt
                });

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoomCleaningSessionResponseDto>>
            GetCleaningSession(int id)
        {
            var session =
                await _cleaningRepository.GetCleaningSessionByIdAsync(id);

            if (session == null)
            {
                return NotFound(new
                {
                    message = "Cleaning session tidak ditemukan."
                });
            }

            var response = new RoomCleaningSessionResponseDto
            {
                Id = session.Id,
                RoomId = session.RoomId,
                BookingId = session.BookingId,
                CleaningDuration = session.CleaningDuration,
                CustomDurationMinutes = session.CustomDurationMinutes,
                StartTime = session.StartTime,
                ScheduledEndTime = session.ScheduledEndTime,
                EndTime = session.EndTime,
                IsCompleted = session.IsCompleted,
                CreatedAt = session.CreatedAt
            };

            return Ok(response);
        }

        [HttpPut("{id}/duration")]
        public async Task<ActionResult> SetCleaningDuration(
            int id,
            [FromBody] SetCleaningDurationRequestDto request)
        {
            var session =
                await _cleaningRepository.GetCleaningSessionByIdAsync(id);

            if (session == null)
            {
                return NotFound(new
                {
                    message = "Cleaning session tidak ditemukan."
                });
            }

            if (session.IsCompleted)
            {
                return BadRequest(new
                {
                    message = "Cleaning session sudah selesai."
                });
            }

            var adminIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token Admin tidak valid"
                });
            }

            var allowedDurations = new[]
            {
                "10_MINUTES",
                "20_MINUTES",
                "30_MINUTES",
                "CUSTOM"
            };

            var duration =
                request.CleaningDuration.Trim().ToUpperInvariant();

            if (!allowedDurations.Contains(duration))
            {
                return BadRequest(new
                {
                    message = "Durasi Cleaning Tidak Valid."
                });
            }

            if (duration == "CUSTOM")
            {
                if (!request.CustomDurationMinutes.HasValue ||
                    request.CustomDurationMinutes.Value <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Custom Duration harus lebih dari 0 menit."
                    });
                }
            }
            else if (request.CustomDurationMinutes.HasValue)
            {
                return BadRequest(new
                {
                    message = "Custom duration hanya boleh diisi jika CleaningDuration adalah CUSTOM"
                });
            }

            if (session.CleaningDuration == duration &&
                session.CustomDurationMinutes == request.CustomDurationMinutes)
            {
                return BadRequest(new
                {
                    message = "Durasi cleaning sudah menggunakan nilai tersebut"
                });
            }

            var success =
                await _cleaningRepository.SetCleaningDurationAsync(
                    id,
                    duration,
                    request.CustomDurationMinutes);

            if (!success)
            {
                return BadRequest(new
                {
                    message = "Gagal menentukan durasi cleaning"
                });
            }

            var durationDetail = 
                duration == "CUSTOM"
                    ? $"CUSTOM {request.CustomDurationMinutes} menit"
                    : duration;

            await _auditLogService.LogAsync(
                adminId,
                "UPDATE",
                "CLEANING",
                session.Id,
                $"Durasi cleaning Room {session.RoomId} diubah menjadi {durationDetail}.");

            return Ok(new
            {
                message = "Durasi cleaning berhasil ditentukan"
            });
        }
    }
}