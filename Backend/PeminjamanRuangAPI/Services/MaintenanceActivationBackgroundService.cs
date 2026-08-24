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
                            await maintenanceRepository
                                .ActivateMaintenanceWithStatusAsync(
                                    maintenance.Id,
                                    maintenance.RoomId,
                                    maintenance.CreatedByAdminId,
                                    maintenance.Description);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Gagal mengaktifkan scheduled maintenance {MaintenanceId}.",
                                maintenance.Id);
                        }
                    }
                    
                    var maintenancesToComplete =
                        await maintenanceRepository
                            .GetMaintenancesReadyToCompleteAsync();
                    
                    foreach (var maintenance in maintenancesToComplete)
                    {
                        try
                        {
                            await maintenanceRepository
                                .CompleteScheduledMaintenanceWithStatusAsync(
                                    maintenance.Id,
                                    maintenance.RoomId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Gagal menyelesaikan scheduled maintenance {MaintenanceId}.",
                                maintenance.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Terjadi error saat memproses scheduled maintenance.");
                }

                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
            }
        }
    }
}