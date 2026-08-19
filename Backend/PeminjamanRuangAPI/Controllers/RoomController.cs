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

        public RoomController(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
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
    }
}