using PeminjamanRuangAPI.Data;
using PeminjamanRuangAPI.Models;
using PeminjamanRuangAPI.Repositories;

public enum BookingApprovalResult
{
    Success,
    InvalidState,
    RoomInactive,
    MaintenanceConflict,
    BookingConflict
}

public enum BookingCreationResult
{
    Success,
    InvalidState,
    RoomInactive,
    MaintenanceConflict,
    BookingConflict
}

namespace PeminjamanRuangAPI.Services
{
    public class BookingTransactionService
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly IBookingRepository _bookingRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IBookingCancellationRepository _bookingCancellationRepository;
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IUserRepository _userRepository;

        public BookingTransactionService(
            DatabaseConnection dbConnection,
            IBookingRepository bookingRepository,
            IAuditLogRepository auditLogRepository,
            INotificationRepository notificationRepository,
            IBookingCancellationRepository bookingCancellationRepository,
            IMaintenanceRepository maintenanceRepository,
            IUserRepository userRepository)
        {
            _dbConnection = dbConnection;
            _bookingRepository = bookingRepository;
            _auditLogRepository = auditLogRepository;
            _notificationRepository = notificationRepository;
            _bookingCancellationRepository = bookingCancellationRepository;
            _maintenanceRepository = maintenanceRepository;
            _userRepository = userRepository;
        }

        public async Task<BookingCreationResult> CreateBookingAsync(
            Booking booking,
            bool notifyAdmins)
        {
            var admins = notifyAdmins
                ? await _userRepository.GetUsersByRoleAsync("ADMIN")
                : [];
        
            await using var connection =
                _dbConnection.CreateConnection();
        
            await connection.OpenAsync();
        
            await using var transaction =
                await connection.BeginTransactionAsync();
        
            try
            {
                var roomLocked =
                    await _bookingRepository.LockRoomAsync(
                        booking.RoomId,
                        connection,
                        transaction);
        
                if (!roomLocked)
                {
                    await transaction.RollbackAsync();
                    return BookingCreationResult.InvalidState;
                }
        
                var roomIsActive =
                    await _bookingRepository.GetRoomActiveStateAsync(
                        booking.RoomId,
                        connection,
                        transaction);
        
                if (roomIsActive != true)
                {
                    await transaction.RollbackAsync();
                    return BookingCreationResult.RoomInactive;
                }
        
                var maintenanceConflict =
                    await _maintenanceRepository.HasMaintenanceConflictAsync(
                        booking.RoomId,
                        booking.BookingDate,
                        connection,
                        transaction);
        
                if (maintenanceConflict)
                {
                    await transaction.RollbackAsync();
                    return BookingCreationResult.MaintenanceConflict;
                }
        
                var bookingConflict =
                    await _bookingRepository.HasBookingConflictAsync(
                        booking.RoomId,
                        booking.BookingDate,
                        booking.StartTime,
                        booking.EndTime,
                        null,
                        connection,
                        transaction);
        
                if (bookingConflict)
                {
                    await transaction.RollbackAsync();
                    return BookingCreationResult.BookingConflict;
                }
        
                var bookingId =
                    await _bookingRepository.CreateBookingAsync(
                        booking,
                        connection,
                        transaction);
        
                if (bookingId <= 0)
                {
                    await transaction.RollbackAsync();
                    return BookingCreationResult.InvalidState;
                }
        
                booking.Id = bookingId;
        
                if (notifyAdmins)
                {
                    foreach (var admin in admins)
                    {
                        var notification = new Notification
                        {
                            UserId = admin.Id,
                            BookingId = booking.Id,
                            NotificationType = "BOOKING_PENDING",
                            EmailSent = false,
                            SentAt = null
                        };
        
                        var notificationCreated =
                            await _notificationRepository.CreateNotificationAsync(
                                notification,
                                connection,
                                transaction);
                        
                        if (!notificationCreated)
                        {
                            await transaction.RollbackAsync();
                            return BookingCreationResult.InvalidState;
                        }
                    }
                }
        
                await transaction.CommitAsync();
        
                return BookingCreationResult.Success;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        public async Task<BookingApprovalResult> ApproveBookingAsync(
            int bookingId,
            int adminId,
            int userId)
        {
            await using var connection = _dbConnection.CreateConnection();
            await connection.OpenAsync();
        
            await using var transaction =
                await connection.BeginTransactionAsync();
        
            try
            {
                var booking =
                    await _bookingRepository.GetBookingByIdAsync(
                        bookingId,
                        connection,
                        transaction);
                
                if (booking == null)
                {
                    await transaction.RollbackAsync();
                    return BookingApprovalResult.InvalidState;
                }
                
                var roomLocked =
                    await _bookingRepository.LockRoomAsync(
                        booking.RoomId,
                        connection,
                        transaction);
                
                if (!roomLocked)
                {
                    await transaction.RollbackAsync();
                    return BookingApprovalResult.InvalidState;
                }

                var roomIsActive =
                    await _bookingRepository.GetRoomActiveStateAsync(
                        booking.RoomId,
                        connection,
                        transaction);
                
                if (roomIsActive != true)
                {
                    await transaction.RollbackAsync();
                    return BookingApprovalResult.RoomInactive;
                }

                booking =
                    await _bookingRepository.GetBookingByIdAsync(
                        bookingId,
                        connection,
                        transaction);
                
                if (booking == null || booking.Status != "PENDING")
                {
                    await transaction.RollbackAsync();
                    return BookingApprovalResult.InvalidState;
                }
                
                var maintenanceConflict =
                    await _maintenanceRepository.HasMaintenanceConflictAsync(
                        booking.RoomId,
                        booking.BookingDate,
                        connection,
                        transaction);
                
                if (maintenanceConflict)
                {
                    await transaction.RollbackAsync();
                    return BookingApprovalResult.MaintenanceConflict;
                }
                
                var bookingConflict =
                    await _bookingRepository.HasApprovedBookingConflictAsync(
                        booking.RoomId,
                        booking.BookingDate,
                        booking.StartTime,
                        booking.EndTime,
                        booking.Id,
                        connection,
                        transaction);
                
                if (bookingConflict)
                {
                    await transaction.RollbackAsync();
                    return BookingApprovalResult.BookingConflict;
                }
                var bookingUpdated =
                    await _bookingRepository.ApproveBookingAsync(
                        bookingId,
                        adminId,
                        connection,
                        transaction);
        
                if (!bookingUpdated)
                {
                    await transaction.RollbackAsync();
                    return BookingApprovalResult.InvalidState;
                }
        
                var auditLog = new AuditLog
                {
                    AdminId = adminId,
                    Action = "APPROVE",
                    EntityType = "BOOKING",
                    EntityId = bookingId,
                    Changes = "Status berubah dari PENDING menjadi APPROVED"
                };
        
                var auditCreated =
                    await _auditLogRepository.CreateAuditLogAsync(
                        auditLog,
                        connection,
                        transaction);
        
                if (!auditCreated)
                {
                    await transaction.RollbackAsync();
                    return BookingApprovalResult.InvalidState;
                }
        
                var notification = new Notification
                {
                    UserId = userId,
                    BookingId = bookingId,
                    NotificationType = "BOOKING_APPROVED",
                    EmailSent = false,
                    SentAt = null
                };
        
                var notificationCreated =
                    await _notificationRepository.CreateNotificationAsync(
                        notification,
                        connection,
                        transaction);
        
                if (!notificationCreated)
                {
                    await transaction.RollbackAsync();
                    return BookingApprovalResult.InvalidState;
                }
        
                await transaction.CommitAsync();
        
                return BookingApprovalResult.Success;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> RejectBookingAsync(
            int bookingId,
            int adminId,
            int userId,
            string reason)
        {
            await using var connection = _dbConnection.CreateConnection();
            await connection.OpenAsync();
        
            await using var transaction =
                await connection.BeginTransactionAsync();
        
            try
            {
                var bookingUpdated =
                    await _bookingRepository.RejectBookingAsync(
                        bookingId,
                        adminId,
                        reason,
                        connection,
                        transaction);
        
                if (!bookingUpdated)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
        
                var auditLog = new AuditLog
                {
                    AdminId = adminId,
                    Action = "REJECT",
                    EntityType = "BOOKING",
                    EntityId = bookingId,
                    Changes =
                        $"Status berubah dari PENDING menjadi REJECTED. Alasan : {reason}"
                };
        
                var auditCreated =
                    await _auditLogRepository.CreateAuditLogAsync(
                        auditLog,
                        connection,
                        transaction);
        
                if (!auditCreated)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
        
                var notification = new Notification
                {
                    UserId = userId,
                    BookingId = bookingId,
                    NotificationType = "BOOKING_REJECTED",
                    EmailSent = false,
                    SentAt = null
                };
        
                var notificationCreated =
                    await _notificationRepository.CreateNotificationAsync(
                        notification,
                        connection,
                        transaction);
        
                if (!notificationCreated)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
        
                await transaction.CommitAsync();
        
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CancelBookingAsync(
            int bookingId,
            int userId,
            int bookingOwnerUserId,
            string reason)
        {
            await using var connection = _dbConnection.CreateConnection();
            await connection.OpenAsync();
        
            await using var transaction =
                await connection.BeginTransactionAsync();
        
            try
            {
                var cancellation = new BookingCancellation
                {
                    BookingId = bookingId,
                    CancellationReason = reason,
                    CancelledByUserId = userId
                };
        
                var cancellationSaved =
                    await _bookingCancellationRepository.CreateCancellationAsync(
                        cancellation,
                        connection,
                        transaction);
        
                if (!cancellationSaved)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
        
                var bookingCancelled =
                    await _bookingRepository.CancelBookingAsync(
                        bookingId,
                        connection,
                        transaction);
        
                if (!bookingCancelled)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
        
                var notification = new Notification
                {
                    UserId = bookingOwnerUserId,
                    BookingId = bookingId,
                    NotificationType = "BOOKING_CANCELLED",
                    EmailSent = false,
                    SentAt = null
                };
        
                var notificationCreated =
                    await _notificationRepository.CreateNotificationAsync(
                        notification,
                        connection,
                        transaction);
        
                if (!notificationCreated)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
        
                await transaction.CommitAsync();
        
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}