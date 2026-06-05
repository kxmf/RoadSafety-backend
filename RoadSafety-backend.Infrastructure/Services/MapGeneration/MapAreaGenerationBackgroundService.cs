using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RoadSafety_backend.Infrastructure.Services.MapGeneration;

internal sealed class MapAreaGenerationBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<MapGenerationSettings> options,
    ILogger<MapAreaGenerationBackgroundService> logger) : BackgroundService
{
    private readonly MapGenerationSettings _settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            logger.LogInformation("Map area generation background service is disabled.");
            return;
        }

        if (_settings.Cities.Count == 0)
        {
            logger.LogWarning("Map area generation is enabled but no cities are configured.");
            return;
        }

        var interval = TimeSpan.FromDays(Math.Max(1, _settings.IntervalDays));

        if (_settings.RunOnStartup)
            await GenerateAllCitiesAsync(stoppingToken);

        using var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await GenerateAllCitiesAsync(stoppingToken);
    }

    private async Task GenerateAllCitiesAsync(CancellationToken cancellationToken)
    {
        foreach (var city in _settings.Cities.Where(c => !string.IsNullOrWhiteSpace(c.CityId)))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<MapAreaGenerationService>();

                await service.GenerateCityAsync(city, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Map area generation failed for city {CityId}.", city.CityId);
            }
        }
    }
}
