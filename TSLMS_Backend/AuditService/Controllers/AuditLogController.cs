using AuditService.DTOs;
using AuditService.Helpers;
using AuditService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditService.Controllers
{
    [ApiController]
    [Authorize(Roles = "HRAdmin")]
    [Route("api/v1/audit")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] AuditLogFilterDto filter, CancellationToken cancellationToken)
        {
            var logs = await _auditLogService.GetAsync(filter, cancellationToken);
            return Ok(new ApiResponse<AuditLogPageDto>(logs, "Audit logs fetched successfully."));
        }
    }
}
