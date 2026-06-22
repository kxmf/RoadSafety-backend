using Microsoft.Extensions.Options;
using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Infrastructure.Services.MapGeneration;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Repositories;

public sealed class MapCityRepository(IOptions<MapGenerationSettings> options) : IMapCityRepository
{
    private readonly MapGenerationSettings _settings = options.Value;

    public Task<IReadOnlyCollection<MapCityResponse>> GetSupportedCitiesAsync(CancellationToken cancellationToken)
    {
        var cities = _settings.Cities
            .Where(city => !string.IsNullOrWhiteSpace(city.CityId))
            .Select(city => new MapCityResponse(
                city.CityId.Trim(),
                string.IsNullOrWhiteSpace(city.Name) ? city.CityId.Trim() : city.Name.Trim(),
                new MapCityBboxResponse(city.MinLon, city.MinLat, city.MaxLon, city.MaxLat)))
            .OrderBy(city => city.Name)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<MapCityResponse>>(cities);
    }
}
