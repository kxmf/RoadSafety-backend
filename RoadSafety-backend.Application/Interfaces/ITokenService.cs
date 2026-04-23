using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.SessionAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Application.Interfaces;

public interface ITokenService
{
    public (string AccessTokenHash, DateTime AccessTokenExpirationDateTime) GenerateAccessToken(User user, FamilyMemberRole familyMemberRole);
    public RefreshToken GenerateRefreshToken(UserId userId);
}
