namespace RoadSafety_backend.Application.DTOs.Requests.Maps;

public sealed record GetAlertZonesRequest(string CityId, Guid FamilyId, Guid? ChildId);
