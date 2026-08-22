using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Helpers;
using NotificationService.Services;

namespace NotificationService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/notifications/preferences")]
    public class NotificationPreferencesController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationPreferencesController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var preference = await _notificationService.GetPreferencesAsync(GetCurrentUserId(), cancellationToken);
            return Ok(new ApiResponse<NotificationPreferenceDto>(preference, "Notification preferences fetched successfully."));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateNotificationPreferenceDto dto, CancellationToken cancellationToken)
        {
            var preference = await _notificationService.UpdatePreferencesAsync(GetCurrentUserId(), dto, cancellationToken);
            return Ok(new ApiResponse<NotificationPreferenceDto>(preference, "Notification preferences updated successfully."));
        }

        private Guid GetCurrentUserId()
        {
            var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(subject!);
        }
    }
}
