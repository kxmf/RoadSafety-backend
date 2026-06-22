namespace RoadSafety_backend.Application.DTOs.Responses.Family;

public record GetFamilyResponse(Guid Id, string? Name, Guid CreatedByUserId, string CityId);
