using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeminjamanRuangAPI.DTOs;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomStatusHistoryRepository _roomStatusRepository;

        public MaintenanceController(
            IMaintenanceRepository maintenanceRepository,
            IRoomRepository roomRepository,
            IBookingRepository bookingRepository,
            IRoomStatusHistoryRepository roomStatusRepository)
        {
            _maintenanceRepository = maintenanceRepository;
            _roomRepository = roomRepository;
            _bookingRepository = bookingRepository;
            _roomStatusRepository = roomStatusRepository;
        }

        [HttpPost]
        public async Task<ActionResult> CreateMaintenance(
            [FromBody] CreateMaintenanceRequestDto request)
        {
            var room = await _roomRepository.GetRoomByIdAsync(request.RoomId);

            if (room == null)
            {
                return NotFound(new
                {
                    message = "Room tidak ditemukan."
                });
            }

            if (request.StartDate <
                DateOnly.FromDateTime(DateTime.Today))
            {
                return BadRequest(new
                {
                    message = "Tanggal mulai maintenance tidak boleh di masa lalu."
                });
            }

            if (request.EndDate.HasValue &&
                request.EndDate.Value < request.StartDate)
            {
                return BadRequest(new
                {
                    message = "Tanggal selesai maintenance tidak boleh sebelum tanggal mulai."
                });
            }

            var allowedPriorities = new[]
            {
                "LOW",
                "MEDIUM",
                "HIGH"
            };

            var priority =
                request.PriorityLevel.Trim().ToUpperInvariant();

            if (!allowedPriorities.Contains(priority))
            {
                return BadRequest(new
                {
                    message = "Priority level harus LOW, MEDIUM, atau HIGH."
                });
            }

            var currentlyInUse =
                await _bookingRepository
                    .IsRoomCurrentlyInUseAsync(request.RoomId);

            if (currentlyInUse)
            {
                return Conflict(new
                {
                    message = "Room sedang digunakan dan tidak dapat masuk maintenance."
                });
            }

            var bookingConflict =
                await _bookingRepository
                    .HasBookingConflictInDateRangeAsync(
                        request.RoomId,
                        request.StartDate,
                        request.EndDate);

            if (bookingConflict)
            {
                return Conflict(new
                {
                    message = "Maintenance bertabrakan dengan booking yang sudah terdaftar."
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

            var maintenance = new Maintenance
            {
                RoomId = request.RoomId,
                MaintenanceCategory = request.MaintenanceCategory.Trim(),
                PriorityLevel = priority,
                FacilitiesServiced = request.FacilitiesServiced?.Trim(),
                Documentation = request.Documentation?.Trim(),
                Description = request.Description.Trim(),
                CreatedByAdminId = adminId,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            var maintenanceId =
                await _maintenanceRepository
                    .CreateMaintenanceAsync(maintenance);

            if (maintenanceId <= 0)
            {
                return BadRequest(new
                {
                    message = "Maintenance gagal dibuat."
                });
            }

            var statusChanged =
                await _roomStatusRepository.ChangeRoomStatusAsync(
                    request.RoomId,
                    "MAINTENANCE",
                    request.Description.Trim(),
                    adminId);

            if (!statusChanged)
            {
                return BadRequest(new
                {
                    message = "Maintenance berhasil dibuat tetapi status room gagal diperbarui."
                });
            }

            return Ok(new
            {
                message = "Maintenance berhasil dibuat.",
                maintenanceId
            });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaintenanceResponseDto>>> GetAllMaintenance()
        {
            var maintenance =
                await _maintenanceRepository.GetAllMaintenancesAsync();

            var response = maintenance.Select(maintenance =>
                new MaintenanceResponseDto
                {
                    Id = maintenance.Id,
                    RoomId = maintenance.RoomId,
                    MaintenanceCategory = maintenance.MaintenanceCategory,
                    PriorityLevel = maintenance.PriorityLevel,
                    FacilitiesServiced = maintenance.FacilitiesServiced,
                    Documentation = maintenance.Documentation,
                    Description = maintenance.Description,
                    CreatedByAdminId = maintenance.CreatedByAdminId,
                    StartDate = maintenance.StartDate,
                    EndDate = maintenance.EndDate,
                    CreatedAt = maintenance.CreatedAt,
                    CompletedAt = maintenance.CompletedAt
                });
            
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceResponseDto>> GetMaintenance (int Id)
        {
            var maintenance =
                await _maintenanceRepository.GetMaintenanceByIdAsync(Id);

            if (maintenance == null)
            {
                return NotFound(new
                {
                    message = "Maintenance tidak ditemukan"
                });
            }

            var response = new MaintenanceResponseDto
            {
                Id = maintenance.Id,
                RoomId = maintenance.RoomId,
                MaintenanceCategory = maintenance.MaintenanceCategory,
                PriorityLevel = maintenance.PriorityLevel,
                FacilitiesServiced = maintenance.FacilitiesServiced,
                Documentation = maintenance.Documentation,
                Description = maintenance.Description,
                CreatedByAdminId = maintenance.CreatedByAdminId,
                StartDate = maintenance.StartDate,
                EndDate = maintenance.EndDate,
                CreatedAt = maintenance.CreatedAt,
                CompletedAt = maintenance.CompletedAt
            };

            return Ok(response);
        }

        [HttpPut("{id}/complete")]
        public async Task<ActionResult> CompleteMaintenance(int id)
        {
            var maintenance =
                await _maintenanceRepository.GetMaintenanceByIdAsync(id);
        
            if (maintenance == null)
            {
                return NotFound(new
                {
                    message = "Maintenance tidak ditemukan."
                });
            }
        
            if (maintenance.CompletedAt != null)
            {
                return BadRequest(new
                {
                    message = "Maintenance sudah selesai sebelumnya."
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
                await _maintenanceRepository.CompleteMaintenanceAsync(id);
        
            if (!completed)
            {
                return BadRequest(new
                {
                    message = "Gagal menyelesaikan maintenance."
                });
            }
        
            var statusChanged =
                await _roomStatusRepository.ChangeRoomStatusAsync(
                    maintenance.RoomId,
                    "ACTIVE",
                    "Maintenance selesai.",
                    adminId);
        
            if (!statusChanged)
            {
                return BadRequest(new
                {
                    message = "Maintenance selesai tetapi status room gagal diperbarui."
                });
            }
        
            return Ok(new
            {
                message = "Maintenance berhasil diselesaikan dan room kembali aktif."
            });
        }
    }
}