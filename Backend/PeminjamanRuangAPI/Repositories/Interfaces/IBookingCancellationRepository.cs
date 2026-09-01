using Npgsql;
using PeminjamanRuangAPI.Models;

namespace PeminjamanRuangAPI.Repositories
{
    public interface IBookingCancellationRepository
    {
        Task<BookingCancellation?> GetByBookingIdAsync(int bookingId);

        Task<bool> CreateCancellationAsync(
            BookingCancellation cancellation);
        Task<bool> CreateCancellationAsync(
            BookingCancellation cancellation,
            NpgsqlConnection connection,
            NpgsqlTransaction transaction);
    }

}