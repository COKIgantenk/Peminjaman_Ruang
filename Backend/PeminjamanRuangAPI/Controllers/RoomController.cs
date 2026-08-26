using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public RoomController(
            IRoomRepository roomRepository,
            IFacilityRepository facilityRepository)
        {
            _roomRepository = roomRepository;
            _facilityRepository = facilityRepository;
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
                [FromQuery] int capacity)

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

            var rooms =
                await _roomRepository.GetAvailableRoomsAsync(
                    date,
                    startTime,
                    endTime,
                    capacity);

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

            var room = new Room
            {
                Name = request.Name,
                Location = request.Location,
                Capacity = request.Capacity,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                IsActive = true
            };

            var success = await _roomRepository.CreateRoomAsync(room);

            if (!success)
            {
                return BadRequest(new
                {
                    message = "Room gagal dibuat."
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

            room.Name = request.Name;
            room.Location = request.Location;
            room.Capacity = request.Capacity;
            room.Description = request.Description;
            room.ImageUrl = request.ImageUrl;
            room.IsActive = request.IsActive;

            var success = await _roomRepository.UpdateRoomAsync(room);

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

            var success = await _roomRepository.DeactivateRoomAsync(id);

            if (!success)
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