using System.Linq;
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
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IUserRepository _userRepository;

        public BookingController(
            IBookingRepository bookingRepository,
            IRoomRepository roomRepository,
            IUserRepository userRepository)
        {
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<ActionResult> CreateBooking(
            [FromBody] CreateBookingRequestDto request)
        {
            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Token user tidak valid."
                });
            }

            var room = await _roomRepository.GetRoomByIdAsync(request.RoomId);

            if (room == null)
            {
                return NotFound(new
                {
                    message = "Room tidak ditemukan."
                });
            }

            if (!room.IsActive)
            {
                return BadRequest(new
                {
                    message = "Room sedang tidak aktif."
                });
            }

            if (request.NumPeople <= 0)
            {
                return BadRequest(new
                {
                    message = "Jumlah peserta harus lebih dari 0."
                });
            }

            if (request.NumPeople > room.Capacity)
            {
                return BadRequest(new
                {
                    message = $"Jumlah peserta melebihi kapasitas room ({room.Capacity})."
                });
            }

            if (request.StartTime >= request.EndTime)
            {
                return BadRequest(new
                {
                    message = "Waktu mulai harus lebih awal dari waktu selesai."
                });
            }

            if (request.BookingDate < DateOnly.FromDateTime(DateTime.Today))
            {
                return BadRequest(new
                {
                    message = "Tanggal booking tidak boleh di masa lalu."
                });
            }

            var conflict = await _bookingRepository.HasBookingConflictAsync(
                request.RoomId,
                request.BookingDate,
                request.StartTime,
                request.EndTime);

            if (conflict)
            {
                return Conflict(new
                {
                    message = "Room sudah memiliki booking pada waktu tersebut."
                });
            }

            var booking = new Booking
            {
                UserId = userId,
                RoomId = request.RoomId,
                BookingDate = request.BookingDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                NumPeople = request.NumPeople,
                Title = request.Title,
                RequesterName = request.RequesterName,
                RequesterDivision = request.RequesterDivision,
                Description = request.Description,

                Status = "PENDING",

                ApprovalNotes = null,
                ApprovedByAdminId = null
            };

            var success = await _bookingRepository.CreateBookingAsync(booking);

            if (!success)
            {
                return BadRequest(new
                {
                    message = "Booking gagal dibuat."
                });
            }

            return Ok(new
            {
                message = "Booking berhasil dibuat dan menunggu persetujuan admin."
            });
        }

        [HttpPost("admin")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> CreateBookingByAdmin(
            [FromBody] AdminCreateBookingRequestDto request)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User tidak ditemukan."
                });
            }

            if (!user.IsActive)
            {
                return BadRequest(new
                {
                    message = "User sedang tidak aktif."
                });
            }

            var room = await _roomRepository.GetRoomByIdAsync(request.RoomId);

            if (room == null)
            {
                return NotFound(new
                {
                    message = "Room tidak ditemukan."
                });
            }

            if (!room.IsActive)
            {
                return BadRequest(new
                {
                    message = "Room sedang tidak aktif."
                });
            }

            if (request.NumPeople <= 0)
            {
                return BadRequest(new
                {
                    message = "Jumlah peserta harus lebih dari 0."
                });
            }

            if (request.NumPeople > room.Capacity)
            {
                return BadRequest(new
                {
                    message = $"Jumlah peserta melebihi kapasitas room ({room.Capacity})."
                });
            }

            if (request.StartTime >= request.EndTime)
            {
                return BadRequest(new
                {
                    message = "Waktu mulai harus lebih awal dari waktu selesai."
                });
            }

            if (request.BookingDate < DateOnly.FromDateTime(DateTime.Today))
            {
                return BadRequest(new
                {
                    message = "Tanggal booking tidak boleh di masa lalu."
                });
            }

            var conflict = await _bookingRepository.HasBookingConflictAsync(
                request.RoomId,
                request.BookingDate,
                request.StartTime,
                request.EndTime);

            if (conflict)
            {
                return Conflict(new
                {
                    message = "Room sudah memiliki booking pada waktu tersebut."
                });
            }

            var booking = new Booking
            {
                UserId = request.UserId,
                RoomId = request.RoomId,
                BookingDate = request.BookingDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                NumPeople = request.NumPeople,
                Title = request.Title,
                RequesterName = request.RequesterName,
                RequesterDivision = request.RequesterDivision,
                Description = request.Description,
                Status = "PENDING",
                ApprovalNotes = null,
                ApprovedByAdminId = null
                    
            };

            var success = await _bookingRepository.CreateBookingAsync(booking);

            if (!success)
            {
                return BadRequest(new
                {
                    message = "Booking gagal dibuat."
                });
            }

            return Ok(new
            {
                message = "Booking berhasil dibuat oleh admin dan menunggu persetujuan."
            });
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetMyBookings()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Token user tidak valid."
                });
            }

            var bookings = await _bookingRepository.GetUserBookingsAsync(userId);

            var response = bookings.Select(booking => new BookingResponseDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                RoomId = booking.RoomId,
                BookingDate = booking.BookingDate,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                NumPeople = booking.NumPeople,
                Title = booking.Title,
                RequesterName = booking.RequesterName,
                RequesterDivision = booking.RequesterDivision,
                Description = booking.Description,
                Status = booking.Status,
                ApprovalNotes = booking.ApprovalNotes,
                ApprovedByAdminId = booking.ApprovedByAdminId,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            });

            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetBooking(int id)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(id);

            if (booking == null)
            {
                return NotFound(new
                {
                    message = "Booking tidak ditemukan."
                });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Token user tidak valid."
                });
            }

            var isAdmin = User.IsInRole("ADMIN");

            if (!isAdmin && booking.UserId != userId)
            {
                return Forbid();
            }

            var response = new BookingResponseDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                RoomId = booking.RoomId,
                BookingDate = booking.BookingDate,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                NumPeople = booking.NumPeople,
                Title = booking.Title,
                RequesterName = booking.RequesterName,
                RequesterDivision = booking.RequesterDivision,
                Description = booking.Description,
                Status = booking.Status,
                ApprovalNotes = booking.ApprovalNotes,
                ApprovedByAdminId = booking.ApprovedByAdminId,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            };

            return Ok(response);
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetAllBookings()
        {
            var bookings = await _bookingRepository.GetAllBookingsAsync();

            var response = bookings.Select(booking => new BookingResponseDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                RoomId = booking.RoomId,
                BookingDate = booking.BookingDate,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                NumPeople = booking.NumPeople,
                Title = booking.Title,
                RequesterName = booking.RequesterName,
                RequesterDivision = booking.RequesterDivision,
                Description = booking.Description,
                Status = booking.Status,
                ApprovalNotes = booking.ApprovalNotes,
                ApprovedByAdminId = booking.ApprovedByAdminId,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            });

            return Ok(response);
        }
        
    }
}