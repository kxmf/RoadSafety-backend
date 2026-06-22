namespace RoadSafety_backend.Application.DTOs.Responses.Tracking;

public sealed record ChildLocationsResponse(IReadOnlyCollection<ChildLocationResponse> Children);
