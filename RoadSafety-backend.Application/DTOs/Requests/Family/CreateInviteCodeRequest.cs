using System.ComponentModel.DataAnnotations;

namespace RoadSafety_backend.Application.DTOs.Requests.Family;

public record CreateInviteCodeRequest()
{
    [Required(ErrorMessage = "Invite code role is required.")]
    public required string InviteCodeRole { get; init; }
}
