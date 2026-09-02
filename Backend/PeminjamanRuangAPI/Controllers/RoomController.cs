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
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IFacilityRepository _facilityRepository;
        private readonly RoomTransactionService _roomTransactionService;
        private readonly RoomStatusTransactionService _roomStatusTransactionService;    

        public RoomController(
            IRoomRepository roomRepository,
            IFacilityRepository facilityRepository,
            RoomTransactionService roomTransactionService,
            RoomStatusTransactionService roomStatusTransactionService)
        {
            _roomRepository = roomRepository;
            _facilityRepository = facilityRepository;
            _roomTransactionService = roomTransactionService;
            _roomStatusTransactionService = roomStatusTransactionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetAllRooms()
        {
            var rooms = await _roomRepository.GetAllRoomsAsync();

            var response = rooms.Select(room => new RoomResponseDto
            {
                Id = room.Id,
                Name = room.Name,
                Location = room.Location,
                Capacity = room.Capacity,
                Description = room.Description,
                ImageUrl = room.ImageUrl,
                IsActive = room.IsActive,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<RoomResponseDto>>>
            GetAvailableRooms(
                [FromQuery] DateTime date,
                [FromQuery] TimeSpan startTime,
                [FromQuery] TimeSpan endTime,
                [FromQuery] int capacity,
                [FromQuery] int[]? facilityIds)

        {
            if (date.Date < DateTime.Today)
            {
                return BadRequest(new
                {
                    message = "Tanggal booking tidak boleh di masa lalu."
                });
            }

            if (startTime >= endTime)
            {
                return BadRequest(new
                {
                    message = "Jam mulai harus lebih awal dari jam selesai."
                });
            }

            if (capacity <= 0)
            {
                return BadRequest(new
                {
                    message = "Capacity harus lebih dari 0."
                });
            }

            var normalizedFacilityIds =
                facilityIds?  
                    .Distinct()
                    .ToArray()
                ?? Array.Empty<int>();

            if(normalizedFacilityIds.Any(id => id <= 0))
            {
                return BadRequest(new
                {
                    message = "Facility ID harus lebih dari 0."
                });
            }

            foreach (var facilityId in normalizedFacilityIds)
            {
                var facility =
                    await _facilityRepository.GetFacilityByIdAsync(facilityId);

                if (facility == null)
                {
                    return BadRequest(new
                    {
                        message = $"Facility dengan ID {facilityId} tidak ditemukan."
                    });
                }
            }

            var rooms =
                await _roomRepository.GetAvailableRoomsAsync(
                    date,
                    startTime,
                    endTime,
                    capacity,
                    normalizedFacilityIds);

            var response = rooms.Select(room =>
                new RoomResponseDto
                {
                    Id = room.Id,
                    Name = room.Name,
                    Location = room.Location,
                    Capacity = room.Capacity,
                    Description = room.Description,
                    ImageUrl = room.ImageUrl,
                    IsActive = room.IsActive,
                    CreatedAt = room.CreatedAt,
                    UpdatedAt = room.UpdatedAt
                });

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoomResponseDto>> GetRoom(int id)
        {
            var room = await _roomRepository.GetRoomByIdAsync(id);

            if (room == null)
            {
                return NotFound(new
                {
                    message = "Room tidak ditemukan."
                });
            }

            var response = new RoomResponseDto
            {
                Id = room.Id,
                Name = room.Name,
                Location = room.Location,
                Capacity = room.Capacity,
                Description = room.Description,
                ImageUrl = room.ImageUrl,
                IsActive = room.IsActive,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> CreateRoom(
            [FromBody] CreateRoomRequestDto request)
        {
            if (request.Capacity <= 0)
            {
                return BadRequest(new
                {
                    message = "Capacity harus lebih dari 0."
                });
            }

            var adminIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token Admin tidak valid."
                });
            }

            var room = new Room
            {
                Name = request.Name.Trim(),
                Location = request.Location.Trim(),
                Capacity = request.Capacity,
                Description = request.Description.Trim(),
                ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl)
                    ? null
                    : request.ImageUrl.Trim(),
                IsActive = true
            };

            var roomId =
                await _roomTransactionService.CreateRoomAsync(
                    room,
                    adminId);
            
            if (roomId <= 0)
            {
                return BadRequest(new
                {
                    message = "Room gagal dibuat"
                });
            }

            return Ok(new
            {
                message = "Room berhasil dibuat."
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> UpdateRoom(
            int id,
            [FromBody] UpdateRoomRequestDto request)
        {
            var room = await _roomRepository.GetRoomByIdAsync(id);

            var adminIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token Admin tidak valid."
                });
            }

            if (room == null)
            {
                return NotFound(new
                {
                    message = "Room tidak ditemukan."
                });
            }

            if (request.Capacity <= 0)
            {
                return BadRequest(new
                {
                    message = "Capacity harus lebih dari 0."
                });
            }

            var oldName = room.Name;
            var oldLocation = room.Location;
            var oldCapacity = room.Capacity;

            room.Name = request.Name.Trim();
            room.Location = request.Location.Trim();
            room.Capacity = request.Capacity;
            room.Description = request.Description.Trim();
            room.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl)
                ? null
                : request.ImageUrl.Trim();

            var success =
                await _roomTransactionService.UpdateRoomAsync(
                    room,
                    adminId,
                    oldName,
                    oldLocation,
                    oldCapacity);
            
            if (!success)
            {
                return BadRequest(new
                {
                    message = "Room gagal diperbarui."
                });
            }

            return Ok(new
            {
                message = "Room berhasil diperbarui."
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> DeactivateRoom(int id)
        {
            var room = await _roomRepository.GetRoomByIdAsync(id);

            if (room == null)
            {
                return NotFound(new
                {
                    message = "Room tidak ditemukan."
                });
            }

            var adminIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token Admin tidak valid."
                });
            }
            
            var result =
                await _roomStatusTransactionService.ChangeRoomStatusAsync(
                    id,
                    "OUT_OF_SERVICE",
                    null,
                    adminId);
            
            if (result == -1)
            {
                return BadRequest(new
                {
                    message = "Room sudah berstatus OUT_OF_SERVICE."
                });
            }
            
            if (result == -2)
            {
                return Conflict(new
                {
                    message =
                        "Room tidak dapat dinonaktifkan karena sedang maintenance atau cleaning."
                });
            }
            
            if (result == -3)
            {
                return Conflict(new
                {
                    message =
                        "Room sedang digunakan dan tidak dapat dinonaktifkan."
                });
            }
            
            if (result == 0)
            {
                return BadRequest(new
                {
                    message = "Room gagal dinonaktifkan."
                });
            }
            
            return Ok(new
            {
                message = "Room berhasil dinonaktifkan."
            });
        }

        [HttpGet("{id}/facilities")]
        public async Task<ActionResult<IEnumerable<FacilityResponseDto>>> GetRoomFacilities(int id)
        {
            var room = await _roomRepository.GetRoomByIdAsync(id);

            if (room == null)
            {
                return NotFound(new
                {
                    message = "Room tidak ditemukan."
                });
            }

            var facilities = await _roomRepository.GetRoomFacilitiesAsync(id);

            var response = facilities.Select(facility => new FacilityResponseDto
            {
                Id = facility.Id,
                Name = facility.Name,
                Description = facility.Description,
                CreatedAt = facility.CreatedAt,
                UpdatedAt = facility.UpdatedAt
            });

            return Ok(response);
        }

        [HttpPost("{roomId}/facilities/{facilityId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> AddFacilityToRoom(int roomId, int facilityId)
        {
            var room = await _roomRepository.GetRoomByIdAsync(roomId);

            if (room == null )
            {
                return NotFound(new
                {
                    message = "Room tidak ditemukan."
                });
            }

            var facility = await _facilityRepository.GetFacilityByIdAsync(facilityId);

            if (facility == null)
            {
                return NotFound(new
                {
                    message = "Facility tidak ditemukan."
                });
            }

            var success = await _roomRepository.AddFacilityToRoomAsync(roomId, facilityId);

            if (!success)
            {
                return BadRequest(new
                {
                    message = "Gagal menambahkan Facility ke Room."
                });
            }

            return Ok(new
            {
                message = "Facility berhasil ditambahkan ke Room."
            });

        }

        [HttpDelete("{roomId}/facilities/{facilityId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> RemoveFacilityFromRoom(int roomId, int facilityId)
        {
            var room = await _roomRepository.GetRoomByIdAsync(roomId);

            if (room == null)
            {
                return NotFound(new
                {
                    message = "Room tidak ditemukan."
                });
            }

            var success = await _roomRepository.RemoveFacilityFromRoomAsync(roomId, facilityId);
            
            if (!success)
            {
                return BadRequest(new
                {
                    message = "Gagal menghapus Facility dari Room."
                });
            }

            return Ok(new
            {
                message = "Facility berhasil dihapus dari Room."
            });
        }
    }
}