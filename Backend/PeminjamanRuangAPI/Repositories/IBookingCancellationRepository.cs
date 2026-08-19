using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IBookingCancellationRepository
    {
        Task<BookingCancellation?> GetByBookingIdAsync(int bookingId);

        Task<bool> CreateCancellationAsync(
            BookingCancellation cancellation);
    }
}