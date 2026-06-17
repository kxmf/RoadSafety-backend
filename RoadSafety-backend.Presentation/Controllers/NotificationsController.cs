using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadSafety_backend.Application.DTOs.Requests.Notifications;
using RoadSafety_backend.Application.DTOs.Responses.Notifications;
using RoadSafety_backend.Application.UseCases.Notifications;
using RoadSafety_backend.Presentation.Extensions;

namespace RoadSafety_backend.Presentation.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(
    GetNotificationsUseCase getNotificationsUseCase,
    MarkNotificationReadUseCase markNotificationReadUseCase,
    RegisterDeviceTokenUseCase registerDeviceTokenUseCase,
    DeleteDeviceTokenUseCase deleteDeviceTokenUseCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(NotificationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false, CancellationToken cancellationToken = default)
    {
        var result = await getNotificationsUseCase.ExecuteAsync(unreadOnly, cancellationToken);
        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await markNotificationReadUseCase.ExecuteAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

        return NoContent();
    }

    [HttpPost("device-tokens")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterDeviceTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await registerDeviceTokenUseCase.ExecuteAsync(request, cancellationToken);
        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

        return NoContent();
    }

    [HttpDelete("device-tokens/{token}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteDeviceToken(string token, CancellationToken cancellationToken)
    {
        var result = await deleteDeviceTokenUseCase.ExecuteAsync(token, cancellationToken);
        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

        return NoContent();
    }
}
