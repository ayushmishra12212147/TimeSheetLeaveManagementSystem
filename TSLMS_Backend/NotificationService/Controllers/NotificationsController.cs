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
    [Route("api/v1/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] NotificationQueryDto query, CancellationToken cancellationToken)
        {
            var notifications = await _notificationService.GetNotificationsAsync(GetCurrentUserId(), query, cancellationToken);
            return Ok(new ApiResponse<NotificationListResponseDto>(notifications, "Notifications fetched successfully."));
        }

        [HttpPatch("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
        {
            await _notificationService.MarkAsReadAsync(GetCurrentUserId(), id, cancellationToken);
            return Ok(new ApiResponse<object>(null, "Notification marked as read."));
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
        {
            await _notificationService.MarkAllAsReadAsync(GetCurrentUserId(), cancellationToken);
            return Ok(new ApiResponse<object>(null, "All notifications marked as read."));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
        {
            var unreadCount = await _notificationService.GetUnreadCountAsync(GetCurrentUserId(), cancellationToken);
            return Ok(new ApiResponse<UnreadCountDto>(unreadCount, "Unread notification count fetched successfully."));
        }

        private Guid GetCurrentUserId()
        {
            var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(subject!);
        }
    }
}
