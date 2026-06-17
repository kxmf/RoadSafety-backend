namespace RoadSafety_backend.Application.DTOs.Responses.Family;

public record MemberDto(Guid Id, string Role, string DisplayName, string Login);

public record GetFamilyMembersResponse(IEnumerable<MemberDto> Members);
