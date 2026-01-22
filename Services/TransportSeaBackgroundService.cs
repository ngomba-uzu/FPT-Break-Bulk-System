// Services/TransportSeaBackgroundService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Break_Bulk_System.Services
{
    public class TransportSeaBackgroundService : BackgroundService
    {
        private readonly ILogger<TransportSeaBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromHours(2);

        public TransportSeaBackgroundService(ILogger<TransportSeaBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TransportSea Background Service started.");

            // Initial delay before first execution (optional)
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting scheduled TransportSea CSV update...");
                    await DownloadAndProcessCsvAsync();
                    _logger.LogInformation("Successfully completed TransportSea CSV update.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during scheduled TransportSea CSV update");
                }

                // Wait for 2 hours before next execution
                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task DownloadAndProcessCsvAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var csvProcessor = scope.ServiceProvider.GetRequiredService<ITransportSeaCsvProcessor>();

            await csvProcessor.ProcessCsvFromUrlAsync();
        }
    }
}