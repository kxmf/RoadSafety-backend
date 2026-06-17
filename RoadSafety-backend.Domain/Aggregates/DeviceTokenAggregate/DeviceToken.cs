using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.DeviceTokenAggregate;

public class DeviceToken
{
    public Guid Id { get; init; }
    public UserId UserId { get; private set; } = null!;
    public string Token { get; private set; } = string.Empty;
    public DevicePlatform Platform { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    private DeviceToken() { }

    private DeviceToken(
        Guid id,
        UserId userId,
        string token,
        DevicePlatform platform,
        DateTimeOffset createdAt,
        DateTimeOffset lastSeenAt)
    {
        Id = id;
        UserId = userId;
        Token = token;
        Platform = platform;
        CreatedAt = createdAt;
        LastSeenAt = lastSeenAt;
    }

    public static DeviceToken Create(
        Guid id,
        UserId userId,
        string token,
        DevicePlatform platform,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        if (id == Guid.Empty)
            throw new ArgumentException("Device token ID cannot be empty.", nameof(id));

        return new DeviceToken(id, userId, token.Trim(), platform, createdAt, createdAt);
    }

    public void Refresh(UserId userId, DevicePlatform platform, DateTimeOffset lastSeenAt)
    {
        ArgumentNullException.ThrowIfNull(userId);

        UserId = userId;
        Platform = platform;
        LastSeenAt = lastSeenAt;
        RevokedAt = null;
    }

    public void Revoke(DateTimeOffset revokedAt)
    {
        RevokedAt ??= revokedAt;
    }
}
