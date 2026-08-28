using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Services
{
    public class BookingCleaningBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingCleaningBackgroundService> _logger;

        public BookingCleaningBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<BookingCleaningBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var bookingRepository =
                        scope.ServiceProvider
                            .GetRequiredService<IBookingRepository>();

                    var cleaningRepository =
                        scope.ServiceProvider
                            .GetRequiredService<IRoomCleaningSessionRepository>();

                    var finishedBookings =
                        await bookingRepository
                            .GetFinishedBookingsWithoutCleaningAsync();

                    foreach (var booking in finishedBookings)
                    {
                        try
                        {
                            await cleaningRepository
                                .CreateAutomaticCleaningSessionAsync(
                                    booking.RoomId,
                                    booking.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Gagal membuat cleaning otomatis untuk booking {BookingId}.",
                                booking.Id);
                        }
                    }

                    var cleaningSessionsToComplete = 
                        await cleaningRepository
                            .GetCleaningSessionsReadyToCompleteAsync();

                    foreach (var session in cleaningSessionsToComplete)
                    {
                        try
                        {
                            var completed =
                            await cleaningRepository
                                .CompleteAutomaticCleaningWithStatusAsync(
                                    session.Id,
                                    session.RoomId);

                            if (completed)
                            {
                                _logger.LogInformation(
                                    "Cleaning session {CleaningSessionId} selesai otomatis.",
                                    session.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Gagal menyelesaikan cleaning otomatis {CleaningSessionId}.",
                                session.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Terjadi error saat memproses automatic cleaning.");
                }

                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
            }
        }
    }
}