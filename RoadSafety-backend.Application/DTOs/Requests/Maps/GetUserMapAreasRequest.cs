namespace RoadSafety_backend.Application.DTOs.Requests.Maps;

public sealed record GetUserMapAreasRequest(Guid FamilyId, Guid? ChildId);
