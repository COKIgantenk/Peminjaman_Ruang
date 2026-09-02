using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PeminjamanRuangAPI.Repositories;

namespace PeminjamanRuangAPI.Services
{
    public class MaintenanceActivationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MaintenanceActivationBackgroundService> _logger;

        public MaintenanceActivationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<MaintenanceActivationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Maintenance background service dimulai.");
        
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
        
                    var maintenanceRepository =
                        scope.ServiceProvider
                            .GetRequiredService<IMaintenanceRepository>();
        
                    var maintenancesToActivate =
                        await maintenanceRepository
                            .GetMaintenancesReadyToActivateAsync();
        
                    foreach (var maintenance in maintenancesToActivate)
                    {
                        try
                        {
                            var activated =
                                await maintenanceRepository
                                    .ActivateMaintenanceWithStatusAsync(
                                        maintenance.Id,
                                        maintenance.RoomId,
                                        maintenance.CreatedByAdminId,
                                        maintenance.Description);
        
                            if (activated)
                            {
                                _logger.LogInformation(
                                    "Maintenance {MaintenanceId} pada Room {RoomId} berhasil diaktifkan otomatis.",
                                    maintenance.Id,
                                    maintenance.RoomId);
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Maintenance {MaintenanceId} pada Room {RoomId} tidak dapat diaktifkan otomatis. Kemungkinan state sudah berubah.",
                                    maintenance.Id,
                                    maintenance.RoomId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Gagal mengaktifkan scheduled maintenance {MaintenanceId} pada Room {RoomId}.",
                                maintenance.Id,
                                maintenance.RoomId);
                        }
                    }
        
                    var maintenancesToComplete =
                        await maintenanceRepository
                            .GetMaintenancesReadyToCompleteAsync();
        
                    foreach (var maintenance in maintenancesToComplete)
                    {
                        try
                        {
                            var completed =
                                await maintenanceRepository
                                    .CompleteScheduledMaintenanceWithStatusAsync(
                                        maintenance.Id,
                                        maintenance.RoomId);
        
                            if (completed)
                            {
                                _logger.LogInformation(
                                    "Maintenance {MaintenanceId} pada Room {RoomId} selesai otomatis.",
                                    maintenance.Id,
                                    maintenance.RoomId);
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Maintenance {MaintenanceId} pada Room {RoomId} tidak dapat diselesaikan otomatis. Kemungkinan state sudah berubah.",
                                    maintenance.Id,
                                    maintenance.RoomId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Gagal menyelesaikan scheduled maintenance {MaintenanceId} pada Room {RoomId}.",
                                maintenance.Id,
                                maintenance.RoomId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Terjadi error saat memproses scheduled maintenance.");
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
                "Maintenance background service dihentikan.");
        }
    }
}