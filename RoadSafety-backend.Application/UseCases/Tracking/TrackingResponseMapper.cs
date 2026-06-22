using RoadSafety_backend.Application.DTOs.Responses.Tracking;
using RoadSafety_backend.Domain.Aggregates.TrackingAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Application.UseCases.Tracking;

public static class TrackingResponseMapper
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
        var displayName = GetProfileDisplayName(user);
        return displayName.Length > 0 ? displayName : GetLogin(user);
    }

    public static string GetProfileDisplayName(User? user)
    {
        if (user?.Profile is null)
            return string.Empty;

        var parts = new[]
        {
            user.Profile.FirstName,
            user.Profile.LastName
        }
        .Where(part => !string.IsNullOrWhiteSpace(part))
        .Select(part => part!.Trim());

        return string.Join(' ', parts);
    }

    public static string GetLogin(User? user)
    {
        if (user is null)
            return string.Empty;

        return user.Contacts.MailAddress?.Address
               ?? user.Contacts.PhoneNumber?.Value
               ?? string.Empty;
    }
}
