namespace RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

public interface IFamilyRepository
{
    Task<Family?> GetFamilyByIdAsync(FamilyId id, CancellationToken cancellationToken);
    Task<Family> CreateFamilyAsync(Family family, CancellationToken cancellationToken);
    Task<Family> UpdateFamilyAsync(Family family, CancellationToken cancellationToken);
    Task<Family> DeleteFamilyAsync(Family family, CancellationToken cancellationToken);
}
