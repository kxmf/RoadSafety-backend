using RoadSafety_backend.Application.DTOs.Responses.Maps;

namespace RoadSafety_backend.Application.Interfaces;

public interface IMapCityRepository
{
    Task<IReadOnlyCollection<MapCityResponse>> GetSupportedCitiesAsync(CancellationToken cancellationToken);
}
