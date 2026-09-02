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
            _logger.LogInformation(
                "Booking cleaning background service dimulai.");
        
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
                            var cleaningSessionId =
                                await cleaningRepository
                                    .CreateAutomaticCleaningSessionAsync(
                                        booking.RoomId,
                                        booking.Id);
        
                            if (cleaningSessionId > 0)
                            {
                                _logger.LogInformation(
                                    "Cleaning session {CleaningSessionId} dibuat otomatis untuk Booking {BookingId} pada Room {RoomId}.",
                                    cleaningSessionId,
                                    booking.Id,
                                    booking.RoomId);
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Cleaning otomatis tidak dibuat untuk Booking {BookingId} pada Room {RoomId}. Kemungkinan booking sudah diproses atau state berubah.",
                                    booking.Id,
                                    booking.RoomId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Gagal membuat cleaning otomatis untuk Booking {BookingId} pada Room {RoomId}.",
                                booking.Id,
                                booking.RoomId);
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
                                    "Cleaning session {CleaningSessionId} pada Room {RoomId} selesai otomatis.",
                                    session.Id,
                                    session.RoomId);
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Cleaning session {CleaningSessionId} pada Room {RoomId} tidak dapat diselesaikan otomatis. Kemungkinan state sudah berubah.",
                                    session.Id,
                                    session.RoomId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Gagal menyelesaikan cleaning otomatis {CleaningSessionId} pada Room {RoomId}.",
                                session.Id,
                                session.RoomId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Terjadi error saat memproses automatic cleaning.");
                }
        
                try
                {
                    await Task.Delay(
                        TimeSpan.FromMinutes(1),
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        
            _logger.LogInformation(
                "Booking cleaning background service dihentikan.");
        }
    }
}