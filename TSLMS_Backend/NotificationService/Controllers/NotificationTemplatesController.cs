using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Helpers;
using NotificationService.Services;

namespace NotificationService.Controllers
{
    [ApiController]
    [Authorize(Roles = "HRAdmin")]
    [Route("api/v1/notifications/templates")]
    public class NotificationTemplatesController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationTemplatesController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var templates = await _notificationService.GetTemplatesAsync(cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<NotificationTemplateDto>>(templates, "Notification templates fetched successfully."));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNotificationTemplateDto dto, CancellationToken cancellationToken)
        {
            var template = await _notificationService.UpdateTemplateAsync(id, dto, cancellationToken);
            return Ok(new ApiResponse<NotificationTemplateDto>(template, "Notification template updated successfully."));
        }
    }
}
