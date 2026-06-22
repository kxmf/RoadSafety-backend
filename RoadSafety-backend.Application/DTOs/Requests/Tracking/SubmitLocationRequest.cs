namespace RoadSafety_backend.Application.DTOs.Requests.Tracking;

public sealed record SubmitLocationRequest(
    Guid? ChildId,
    double Latitude,
    double Longitude,
    double? AccuracyMeters,
    DateTimeOffset? RecordedAt);
