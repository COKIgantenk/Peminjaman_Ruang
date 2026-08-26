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
        private readonly IBookingCancellationRepository _bookingCancellationRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMaintenanceRepository _maintenanceRepository;

        public BookingController(
            IBookingRepository bookingRepository,
            IRoomRepository roomRepository,
            IUserRepository userRepository,
            IBookingCancellationRepository bookingCancellationRepository,
            INotificationRepository notificationRepository,
            IMaintenanceRepository maintenanceRepository)
        {
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _userRepository = userRepository;
            _bookingCancellationRepository = bookingCancellationRepository;
            _notificationRepository = notificationRepository;
            _maintenanceRepository = maintenanceRepository;
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

            var today = DateOnly.FromDateTime(DateTime.Now);
            var currentTime = TimeOnly.FromDateTime(DateTime.Now);

            if (request.BookingDate == today &&
                request.StartTime <= currentTime)
            {
                return BadRequest(new
                {
                    message = "Waktu mulai booking harus lebih besar dari waktu saat ini"
                });
            }

            var maintenanceConflict =
                await _maintenanceRepository.HasMaintenanceConflictAsync(
                    request.RoomId,
                    request.BookingDate);

            if (maintenanceConflict)
            {
                return Conflict(new
                {
                    message = "Room tidak tersedia karena terdapat jadwal maintenance pada tanggal tersebut."
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

            var bookingId = await _bookingRepository.CreateBookingAsync(booking);

            if (bookingId <= 0)
            {
                return BadRequest(new
                {
                    message = "Booking gagal dibuat."
                });
            }

            booking.Id = bookingId;

            var admins = await _userRepository.GetUsersByRoleAsync("ADMIN");

            foreach (var admin in admins)
            {
                var notification = new Notification
                {
                    UserId = admin.Id,
                    BookingId = booking.Id,
                    NotificationType = "BOOKING_PENDING",
                    EmailSent = false,
                    SentAt = null,
                };

                await _notificationRepository.CreateNotificationAsync(notification);
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

            var today = DateOnly.FromDateTime(DateTime.Now);
            var currentTime = TimeOnly.FromDateTime(DateTime.Now);

            if (request.BookingDate == today &&
                request.StartTime <= currentTime)
            {
                return BadRequest(new
                {
                    message = "Waktu mulai booking harus lebih besar dari waktu saat ini"
                });
            }

            var maintenanceConflict =
                await _maintenanceRepository.HasMaintenanceConflictAsync(
                    request.RoomId,
                    request.BookingDate);

            if (maintenanceConflict)
            {
                return Conflict(new
                {
                    message = "Room tidak tersedia karena terdapat jadwal maintenance pada tanggal tersebut."
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

            var bookingId = await _bookingRepository.CreateBookingAsync(booking);

            if (bookingId <= 0)
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

            return Ok(response);
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

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> ApproveBooking(int id)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(id);

            if (booking == null)
            {
                return NotFound(new
                {
                    message = "Booking tidak ditemukan."
                });
            }

            if (booking.Status != "PENDING")
            {
                return BadRequest(new
                {
                    message = "Hanya booking dengan status PENDING yang dapat disetujui."
                });
            }

            var room = await _roomRepository.GetRoomByIdAsync(booking.RoomId);

            if (room == null)
            {
                return NotFound(new
                {
                    message = "Room tidak ditemukan"
                });
            }

            if (!room.IsActive)
            {
                return Conflict(new
                {
                    message = "Booking tidak dapat disetujui karena room sedang tidak tersedia."
                });
            }

            var maintenanceConflict =
                await _maintenanceRepository.HasMaintenanceConflictAsync(
                    booking.RoomId,
                    booking.BookingDate);

            if (maintenanceConflict)
            {
                return Conflict(new
                {
                    message = "Booking tidak dapat disetujui karena room memiliki jadwal maintenance."
                });
            }

            var bookingConflict = 
                await _bookingRepository.HasBookingConflictAsync(
                    booking.RoomId,
                    booking.BookingDate,
                    booking.StartTime,
                    booking.EndTime,
                    booking.Id);

            if (bookingConflict)
            {
                return Conflict(new
                {
                    message = "Booking tidak dapat disetujui karena terdapat booking lain pada waktu tersebut."
                });
            }

            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token admin tidak valid."
                });
            }

            var success = await _bookingRepository.ApproveBookingAsync(id, adminId);

            if (!success)
            {
                return BadRequest(new
                {
                    message = "Gagal menyetujui booking."
                });
            }

            var notification = new Notification
            {
                UserId = booking.UserId,
                BookingId = booking.Id,
                NotificationType = "BOOKING_APPROVED",
                EmailSent = false,
                SentAt = null,
            };

            var notificationCreated =
                await _notificationRepository.CreateNotificationAsync(notification);

            if (!notificationCreated)
            {
                return BadRequest(new
                {
                    message = "Booking berhasil disetujui, tetapi gagal membuat notifikasi."
                });
            }

            return Ok(new
            {
                message = "Booking berhasil disetujui."
            });
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> RejectBooking(
            int id, 
            [FromBody] RejectBookingRequestDto request)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(id);

            if (booking == null)
            {
                return NotFound(new
                {
                    message = "Booking tidak ditemukan."
                });
            }

            if (booking.Status != "PENDING")
            {
                return BadRequest(new
                {
                    message = "Hanya booking dengan status PENDING yang dapat ditolak."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new
                {
                    message = "Alasan penolakan harus diisi."
                });
            }

            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new
                {
                    message = "Token admin tidak valid."
                });
            }

            var success = await _bookingRepository.RejectBookingAsync(
                id, 
                adminId,
                request.Reason.Trim());

            if (!success)
            {
                return BadRequest(new
                {
                    message = "Booking gagal ditolak."
                });
            }

            var notification = new Notification
            {
                UserId = booking.UserId,
                BookingId = booking.Id,
                NotificationType = "BOOKING_REJECTED",
                EmailSent = false,
                SentAt = null,
            };

            var notificationCreated =
                await _notificationRepository.CreateNotificationAsync(notification);

            if (!notificationCreated)
            {
                return BadRequest(new
                {
                    message = "Booking berhasil ditolak, tetapi gagal membuat notifikasi."
                });
            }

            return Ok(new
            {
                message = "Booking berhasil ditolak."
            });
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult> CancelBooking(
            int id, 
            [FromBody] CancelBookingRequestDto request)
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

            if (booking.UserId != userId)
            {
                return Forbid();
            }

            if (booking.Status!= "PENDING" && booking.Status != "APPROVED")
            {
                return BadRequest(new
                {
                    message = "Hanya booking dengan status PENDING atau APPROVED yang dapat dibatalkan."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new
                {
                    message = "Alasan pembatalan harus diisi."
                });
            }

            var cancellation = new BookingCancellation
            {
                BookingId = id,
                CancellationReason = request.Reason.Trim(),
                CancelledByUserId = userId,
            };

            var cancellationSaved =
                await _bookingCancellationRepository.CreateCancellationAsync(cancellation); 

            if (!cancellationSaved)
            {
                return BadRequest(new
                {
                    message = "Gagal menyimpan alasan pembatalan."
                });
            }

            var cancelled = await _bookingRepository.CancelBookingAsync(id);

            if (!cancelled)
            {
                return BadRequest(new
                {
                    message = "Gagal membatalkan booking."
                });
            }

            var notification = new Notification
            {
                UserId = booking.UserId,
                BookingId = booking.Id,
                NotificationType = "BOOKING_CANCELLED",
                EmailSent = false,
                SentAt = null,
            };

            var notificationCreated =
                await _notificationRepository.CreateNotificationAsync(notification);

            if (!notificationCreated)
            {
                return BadRequest(new
                {
                    message = "Booking berhasil dibatalkan, tetapi gagal membuat notifikasi."
                });
            }

            return Ok(new
            {
                message = "Booking berhasil dibatalkan."
            });
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetBookingsByStatus(
            string status)
        {
            var allowedStatuses = new[] 
            { 
                "PENDING", 
                "APPROVED", 
                "REJECTED", 
                "CANCELLED" 
            };

            var normalizedStatus = status.ToUpperInvariant();

            if (!allowedStatuses.Contains(normalizedStatus))
            {
                return BadRequest(new
                {
                    message = "Status tidak valid"
                });
            }

            var bookings = await _bookingRepository
                .GetBookingsByStatusAsync(normalizedStatus);

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

        [HttpGet("date/{date}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetBookingsByDate(
            DateOnly date)
        {
            var bookings = await _bookingRepository
                .GetBookingsByDateAsync(date);

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