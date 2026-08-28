using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeminjamanRuangAPI.Services;
using PeminjamanRuangAPI.DTOs;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomStatusController : ControllerBase
    {
        private readonly IRoomStatusHistoryRepository _roomStatusRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly AuditLogService _auditLogService;

        public RoomStatusController(
            IRoomStatusHistoryRepository roomStatusRepository,
            IRoomRepository roomRepository,
            IBookingRepository bookingRepository,
            AuditLogService auditLogService)
        {
            _roomStatusRepository = roomStatusRepository;
            _roomRepository = roomRepository;
            _bookingRepository = bookingRepository;
            _auditLogService = auditLogService;
        }

        // ADMIN: mengubah status room
        [HttpPut("{roomId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> ChangeRoomStatus(
            int roomId,
            [FromBody] ChangeRoomStatusRequestDto request)
        {
            var room = await _roomRepository.GetRoomByIdAsync(roomId);

            if (room == null)
            {
                return NotFound(new
                {
                    message = "Room tidak ditemukan."
                });
            }

            var allowedStatuses = new[]
            {
                "ACTIVE",
                "OUT_OF_SERVICE"      
            };

            var status = request.Status.Trim().ToUpperInvariant();

            if (!allowedStatuses.Contains(status))
            {
                return BadRequest(new
                {
                    message = "Status room tidak valid."
                });
            }

            // Room yang sedang digunakan tidak boleh dinonaktifkan.
            if (status != "ACTIVE")
            {
                var currentlyInUse =
                    await _bookingRepository
                        .IsRoomCurrentlyInUseAsync(roomId);

                if (currentlyInUse)
                {
                    return Conflict(new
                    {
                        message =
                            "Room sedang digunakan dan status tidak dapat diubah."
                    });
                }
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

            var previousStatus =
                await _roomStatusRepository
                    .GetLatestRoomStatusAsync(roomId);

            var previousStatusName =
                previousStatus?.Status ?? "UNKNOWN";

            if (previousStatusName == status)
            {
                return BadRequest(new
                {
                    message = $"Room sudah berstatus {status}."
                });
            }

            var success =
                await _roomStatusRepository.ChangeRoomStatusAsync(
                    roomId,
                    status,
                    request.Reason?.Trim(),
                    adminId);

            if (!success)
            {
                return BadRequest(new
                {
                    message = "Gagal mengubah status room."
                });
            }

            var auditAction =
                status == "ACTIVE"
                    ? "ACTIVATE"
                    : "DEACTIVATE";

            await _auditLogService.LogAsync(
                adminId,
                auditAction,
                "ROOM",
                roomId,
                $"Status room berubah dari {previousStatusName} menjadi {status}");

            return Ok(new
            {
                message = "Status room berhasil diperbarui.",
                roomId,
                status
            });
        }

        // Melihat status terakhir sebuah room
        [HttpGet("{roomId}/latest")]
        public async Task<ActionResult<RoomStatusHistoryResponseDto>>
            GetLatestStatus(int roomId)
        {
            var room = await _roomRepository.GetRoomByIdAsync(roomId);

            if (room == null)
            {
                return NotFound(new
                {
                    message = "Room tidak ditemukan."
                });
            }

            var status =
                await _roomStatusRepository
                    .GetLatestRoomStatusAsync(roomId);

            if (status == null)
            {
                return NotFound(new
                {
                    message = "Riwayat status room belum tersedia."
                });
            }

            var response = new RoomStatusHistoryResponseDto
            {
                Id = status.Id,
                RoomId = status.RoomId,
                Status = status.Status,
                Reason = status.Reason,
                ChangedByAdminId = status.ChangedByAdminId,
                CreatedAt = status.CreatedAt
            };

            return Ok(response);
        }

        // Melihat seluruh histori perubahan status sebuah room
        [HttpGet("{roomId}/history")]
        public async Task<ActionResult<IEnumerable<RoomStatusHistoryResponseDto>>>
            GetStatusHistory(int roomId)
        {
            var room = await _roomRepository.GetRoomByIdAsync(roomId);

            if (room == null)
            {
                return NotFound(new
                {
                    message = "Room tidak ditemukan."
                });
            }

            var histories =
                await _roomStatusRepository
                    .GetRoomStatusHistoryAsync(roomId);

            var response = histories.Select(status =>
                new RoomStatusHistoryResponseDto
                {
                    Id = status.Id,
                    RoomId = status.RoomId,
                    Status = status.Status,
                    Reason = status.Reason,
                    ChangedByAdminId = status.ChangedByAdminId,
                    CreatedAt = status.CreatedAt
                });

            return Ok(response);
        }
    }
}