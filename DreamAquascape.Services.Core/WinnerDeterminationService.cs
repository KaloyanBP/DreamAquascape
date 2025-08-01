using DreamAquascape.Services.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core
{
    public class WinnerDeterminationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WinnerDeterminationService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

        public WinnerDeterminationService(IServiceProvider serviceProvider, ILogger<WinnerDeterminationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEndedContests();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing ended contests.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessEndedContests()
        {
            using var scope = _serviceProvider.CreateScope();
            var contestService = scope.ServiceProvider.GetRequiredService<IContestService>();

            try
            {
                var newWinners = await contestService.ProcessEndedContestsAsync();

                if (newWinners.Any())
                {
                    _logger.LogInformation("Determined {Count} new contest winners", newWinners.Count);

                    // TODO:
                    // - Send notification emails to winners
                    // - Update contest status
                    // - Generate announcements
                    foreach (var winner in newWinners)
                    {
                        _logger.LogInformation("Contest {ContestId}: Entry {EntryId} is the winner",
                            winner.ContestId, winner.ContestEntryId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ended contests for winner determination");
            }
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WinnerDeterminationService is stopping.");
            return base.StopAsync(stoppingToken);
        }
    }
}
