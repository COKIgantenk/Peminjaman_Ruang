using Npgsql;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<Booking?> GetBookingByIdAsync(
            int id,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool> LockRoomAsync(
            int roomId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool?> GetRoomActiveStateAsync(
            int roomId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId);
        Task<IEnumerable<Booking>> GetBookingsByStatusAsync(string status);
        Task<IEnumerable<Booking>> GetBookingsByDateAsync(DateOnly date);
        Task<IEnumerable<Booking>> GetFinishedBookingsWithoutCleaningAsync();
        Task<int> CreateBookingAsync(Booking booking);
        Task<int> CreateBookingAsync(
            Booking booking,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool> UpdateBookingAsync(Booking booking);
        Task<bool> ApproveBookingAsync(
            int bookingId, 
            int adminId);
        Task<bool> ApproveBookingAsync(
            int bookingId,
            int adminId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool> RejectBookingAsync(
            int bookingId, 
            int adminId, 
            string reason);
        Task<bool> RejectBookingAsync(
            int bookingId,
            int adminId,
            string reason,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool> CancelBookingAsync(int bookingId);
        Task<bool> CancelBookingAsync(
            int bookingId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool> HasBookingConflictAsync(
            int roomId, 
            DateOnly bookingDate, 
            TimeOnly startTime, 
            TimeOnly endTime,
            int? excludebookingId = null);
        Task<bool> HasBookingConflictAsync(
            int roomId,
            DateOnly bookingDate,
            TimeOnly startTime,
            TimeOnly endTime,
            int? excludeBookingId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool> HasApprovedBookingConflictAsync(
            int roomId,
            DateOnly bookingDate,
            TimeOnly startTime,
            TimeOnly endTime,
            int excludeBookingId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool> IsRoomCurrentlyInUseAsync(int roomId);
        Task<bool> IsRoomCurrentlyInUseAsync(
            int roomId,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
        Task<bool> HasBookingConflictInDateRangeAsync(
            int roomId,
            DateOnly StartDate,
            DateOnly? EndDate);
        Task<bool> HasBookingConflictInDateRangeAsync(
            int roomId,
            DateOnly startDate,
            DateOnly? endDate,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);    
    }
}