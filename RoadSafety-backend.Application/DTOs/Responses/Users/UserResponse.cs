namespace RoadSafety_backend.Application.DTOs.Responses.Users;

public record UserResponse(
    Guid Id,
    string? Email,
    string? PhoneNumber,
    string? FirstName,
    string? LastName,
    string? Patronymic,
    DateOnly? BirthDate,
    Guid? FamilyId,
    string? FamilyRole
);
