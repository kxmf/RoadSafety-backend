using RoadSafety_backend.Application.DTOs.Responses.Tracking;
using RoadSafety_backend.Domain.Aggregates.TrackingAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Application.UseCases.Tracking;

internal static class TrackingResponseMapper
{
    public static ChildLocationResponse ToResponse(ChildLocation location, User? child)
    {
        return new ChildLocationResponse(
            location.ChildId.Value,
            GetDisplayName(child),
            location.Location.Y,
            location.Location.X,
            location.AccuracyMeters,
            location.CurrentRisk,
            location.LastUpdatedAt);
    }

    public static string GetDisplayName(User? user)
    {
        if (user?.Profile is null)
            return string.Empty;

        var parts = new[]
        {
            user.Profile.FirstName,
            user.Profile.LastName
        }.Where(part => !string.IsNullOrWhiteSpace(part));

        return string.Join(' ', parts);
    }
}
