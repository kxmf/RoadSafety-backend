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
        logger.LogInformation(
            "Map area generation background service started. CityCount: {CityCount}, RunOnStartup: {RunOnStartup}, Interval: {Interval}.",
            _settings.Cities.Count(c => !string.IsNullOrWhiteSpace(c.CityId)),
            _settings.RunOnStartup,
            interval);

        if (_settings.RunOnStartup)
            await GenerateAllCitiesAsync(stoppingToken);

        using var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await GenerateAllCitiesAsync(stoppingToken);
    }

    private async Task GenerateAllCitiesAsync(CancellationToken cancellationToken)
    {
        var cities = _settings.Cities.Where(c => !string.IsNullOrWhiteSpace(c.CityId)).ToList();
        logger.LogInformation("Starting map area generation run. CityCount: {CityCount}.", cities.Count);

        foreach (var city in cities)
        {
            try
            {
                logger.LogInformation("Starting map area generation city run. CityId: {CityId}.", city.CityId);

                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<MapAreaGenerationService>();

                await service.GenerateCityAsync(city, cancellationToken);

                logger.LogInformation("Finished map area generation city run. CityId: {CityId}.", city.CityId);
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
