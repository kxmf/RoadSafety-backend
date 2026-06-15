namespace RoadSafety_backend.Application.DTOs.Responses.Tracking;

public sealed record ChildStatsResponse(Guid ChildId, int TotalScore, int Rating);
