using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetService.DTOs;
using TimesheetService.Helpers;
using TimesheetService.Services;

namespace TimesheetService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/timesheets")]
    public class TimesheetEntriesController : ControllerBase
    {
        private readonly ITimesheetEntryService _timesheetEntryService;

        public TimesheetEntriesController(ITimesheetEntryService timesheetEntryService)
        {
            _timesheetEntryService = timesheetEntryService;
        }

        [HttpGet("week")]
        public async Task<IActionResult> GetWeek([FromQuery] DateOnly? weekStartDate, [FromQuery] string? employeeId, CancellationToken cancellationToken)
        {
            var week = await _timesheetEntryService.GetWeekAsync(weekStartDate, employeeId, cancellationToken);
            return Ok(new ApiResponse<WeekTimesheetResponseDto>(week, "Timesheet week fetched successfully."));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTimesheetEntryDto dto, CancellationToken cancellationToken)
        {
            var entry = await _timesheetEntryService.CreateAsync(dto, cancellationToken);
            return Ok(new ApiResponse<TimesheetEntryResponseDto>(entry, "Timesheet entry created successfully."));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTimesheetEntryDto dto, CancellationToken cancellationToken)
        {
            var entry = await _timesheetEntryService.UpdateAsync(id, dto, cancellationToken);
            return Ok(new ApiResponse<TimesheetEntryResponseDto>(entry, "Timesheet entry updated successfully."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _timesheetEntryService.DeleteAsync(id, cancellationToken);
            return Ok(new ApiResponse<string>("Timesheet entry deleted successfully."));
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitTimesheetDto dto, CancellationToken cancellationToken)
        {
            var summary = await _timesheetEntryService.SubmitAsync(dto, cancellationToken);
            return Ok(new ApiResponse<WeeklyTimesheetSummaryResponseDto>(summary, "Timesheet submitted successfully."));
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending([FromQuery] DateOnly? weekStartDate, CancellationToken cancellationToken)
        {
            var summaries = await _timesheetEntryService.GetPendingAsync(weekStartDate, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<WeeklyTimesheetSummaryResponseDto>>(summaries, "Pending timesheets fetched successfully."));
        }

        [HttpPatch("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveTimesheetDto dto, CancellationToken cancellationToken)
        {
            var summary = await _timesheetEntryService.ApproveAsync(id, dto, cancellationToken);
            return Ok(new ApiResponse<WeeklyTimesheetSummaryResponseDto>(summary, "Timesheet approved successfully."));
        }

        [HttpPatch("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectTimesheetDto dto, CancellationToken cancellationToken)
        {
            var summary = await _timesheetEntryService.RejectAsync(id, dto, cancellationToken);
            return Ok(new ApiResponse<WeeklyTimesheetSummaryResponseDto>(summary, "Timesheet rejected successfully."));
        }

        [HttpGet("team")]
        public async Task<IActionResult> GetTeam([FromQuery] DateOnly? weekStartDate, [FromQuery] string? employeeId, CancellationToken cancellationToken)
        {
            var summaries = await _timesheetEntryService.GetTeamAsync(weekStartDate, employeeId, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<WeeklyTimesheetSummaryResponseDto>>(summaries, "Team timesheets fetched successfully."));
        }
    }
}
