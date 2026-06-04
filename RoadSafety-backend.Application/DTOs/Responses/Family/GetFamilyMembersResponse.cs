namespace RoadSafety_backend.Application.DTOs.Responses.Family;

public record MemberDto(Guid Id, string Role);

public record GetFamilyMembersResponse(IEnumerable<MemberDto> Members);