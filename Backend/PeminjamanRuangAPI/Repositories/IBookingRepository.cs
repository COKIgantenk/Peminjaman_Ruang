using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId);
        Task<IEnumerable<Booking>> GetBookingsByStatusAsync(string status);
        Task<IEnumerable<Booking>> GetBookingsByDateAsync(DateOnly date);
        Task<int> CreateBookingAsync(Booking booking);
        Task<bool> UpdateBookingAsync(Booking booking);
        Task<bool> ApproveBookingAsync(int bookingId, int adminId);
        Task<bool> RejectBookingAsync(int bookingId, int adminId, string reason);
        Task<bool> CancelBookingAsync(int bookingId);
        Task<bool> HasBookingConflictAsync(
            int roomId, 
            DateOnly bookingDate, 
            TimeOnly startTime, 
            TimeOnly endTime,
            int? excludebookingId = null);
        Task<bool> IsRoomCurrentlyInUseAsync(int roomId);
        Task<bool> HasBookingConflictInDateRangeAsync(
            int roomId,
            DateOnly StartDate,
            DateOnly? EndDate);    
    }
}