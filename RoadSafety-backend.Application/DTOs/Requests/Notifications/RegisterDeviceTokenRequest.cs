using RoadSafety_backend.Domain.Aggregates.DeviceTokenAggregate;

namespace RoadSafety_backend.Application.DTOs.Requests.Notifications;

public sealed record RegisterDeviceTokenRequest(
    string Token,
    DevicePlatform Platform);
