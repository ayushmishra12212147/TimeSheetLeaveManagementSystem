using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetService.DTOs;
using TimesheetService.Helpers;
using TimesheetService.Services;

namespace TimesheetService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/timesheet-config")]
    public class TimesheetConfigController : ControllerBase
    {
        private readonly ITimesheetConfigService _timesheetConfigService;

        public TimesheetConfigController(ITimesheetConfigService timesheetConfigService)
        {
            _timesheetConfigService = timesheetConfigService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var config = await _timesheetConfigService.GetAsync(cancellationToken);
            return Ok(new ApiResponse<TimesheetConfigResponseDto>(config, "Timesheet config fetched successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateTimesheetConfigDto dto, CancellationToken cancellationToken)
        {
            var config = await _timesheetConfigService.UpdateAsync(dto, cancellationToken);
            return Ok(new ApiResponse<TimesheetConfigResponseDto>(config, "Timesheet config updated successfully."));
        }
    }
}
