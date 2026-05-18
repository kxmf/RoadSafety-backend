using RoadSafety_backend.Application.DTOs.Requests.Family;
using RoadSafety_backend.Application.DTOs.Responses.Family;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Family;

public class CreateFamilyUseCase(
    IUnitOfWork unitOfWork,
    IFamilyRepository familyRepository
    )
{
    public Task<Result<CreateFamilyResponse>> ExecuteAsync(CreateFamilyRequest request, CancellationToken cancellationToken)
    {
        var familyMember = new 
    }
}

