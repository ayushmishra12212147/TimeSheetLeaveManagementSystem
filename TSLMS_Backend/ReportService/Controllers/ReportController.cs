using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportService.DTOs;
using ReportService.Exceptions;
using ReportService.Helpers;
using ReportService.Services;

namespace ReportService.Controllers
{
    [ApiController]
    [Authorize(Roles = "Manager,HRAdmin")]
    [Route("api/v1/reports")]
    public class ReportController : ControllerBase
    {
        private readonly IAttendanceReportService _attendanceReportService;
        private readonly ILeaveReportService _leaveReportService;
        private readonly ITimesheetReportService _timesheetReportService;
        private readonly IDashboardReportService _dashboardReportService;
        private readonly IExportService _exportService;
        private readonly IReportRequestService _reportRequestService;

        public ReportController(
            IAttendanceReportService attendanceReportService,
            ILeaveReportService leaveReportService,
            ITimesheetReportService timesheetReportService,
            IDashboardReportService dashboardReportService,
            IExportService exportService,
            IReportRequestService reportRequestService)
        {
            _attendanceReportService = attendanceReportService;
            _leaveReportService = leaveReportService;
            _timesheetReportService = timesheetReportService;
            _dashboardReportService = dashboardReportService;
            _exportService = exportService;
            _reportRequestService = reportRequestService;
        }

        [HttpGet("leave")]
        public async Task<IActionResult> GetLeave([FromQuery] LeaveReportRequestDto query, CancellationToken cancellationToken)
        {
            var report = await _leaveReportService.GenerateAsync(query, cancellationToken);
            return Ok(new ApiResponse<LeaveReportResponseDto>(report, "Leave report generated successfully."));
        }

        [HttpGet("timesheet")]
        public async Task<IActionResult> GetTimesheet([FromQuery] TimesheetReportRequestDto query, CancellationToken cancellationToken)
        {
            var report = await _timesheetReportService.GenerateAsync(query, cancellationToken);
            return Ok(new ApiResponse<TimesheetReportResponseDto>(report, "Timesheet report generated successfully."));
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] DateOnly? dateFrom, [FromQuery] DateOnly? dateTo, [FromQuery] string? employeeId, CancellationToken cancellationToken)
        {
            var report = await _dashboardReportService.GenerateAsync(dateFrom, dateTo, employeeId, cancellationToken);
            return Ok(new ApiResponse<DashboardReportResponseDto>(report, "Dashboard report generated successfully."));
        }

        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendance([FromQuery] AttendanceReportRequestDto query, CancellationToken cancellationToken)
        {
            var report = await _attendanceReportService.GenerateAsync(query, cancellationToken);
            return Ok(new ApiResponse<AttendanceReportResponseDto>(report, "Attendance report generated successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpGet("attendance/export")]
        public async Task<IActionResult> ExportAttendance([FromQuery] AttendanceReportRequestDto query, [FromQuery] ExportRequestDto exportRequest, CancellationToken cancellationToken)
        {
            var report = await _attendanceReportService.GenerateAsync(query, cancellationToken);
            var file = await _exportService.ExportAttendanceAsync(report, exportRequest.Format, cancellationToken);
            return File(file.Content, file.ContentType, file.FileName);
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpGet("leave/export")]
        public async Task<IActionResult> ExportLeave([FromQuery] LeaveReportRequestDto query, [FromQuery] ExportRequestDto exportRequest, CancellationToken cancellationToken)
        {
            var report = await _leaveReportService.GenerateAsync(query, cancellationToken);
            var file = await _exportService.ExportLeaveAsync(report, exportRequest.Format, cancellationToken);
            return File(file.Content, file.ContentType, file.FileName);
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpGet("timesheet/export")]
        public async Task<IActionResult> ExportTimesheet([FromQuery] TimesheetReportRequestDto query, [FromQuery] ExportRequestDto exportRequest, CancellationToken cancellationToken)
        {
            var report = await _timesheetReportService.GenerateAsync(query, cancellationToken);
            var file = await _exportService.ExportTimesheetAsync(report, exportRequest.Format, cancellationToken);
            return File(file.Content, file.ContentType, file.FileName);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetRequests([FromQuery] bool pendingOnly, CancellationToken cancellationToken)
        {
            var requests = await _reportRequestService.GetVisibleAsync(pendingOnly, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<ReportRequestResponseDto>>(requests, "Report requests fetched successfully."));
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("requests")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateReportRequestDto dto, CancellationToken cancellationToken)
        {
            var request = await _reportRequestService.CreateAsync(dto, cancellationToken);
            return Ok(new ApiResponse<ReportRequestResponseDto>(request, "Report request submitted for HR approval."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPatch("requests/{id:guid}/approve")]
        public async Task<IActionResult> ApproveRequest(Guid id, CancellationToken cancellationToken)
        {
            var request = await _reportRequestService.ApproveAsync(id, cancellationToken);
            return Ok(new ApiResponse<ReportRequestResponseDto>(request, "Report request approved successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPatch("requests/{id:guid}/reject")]
        public async Task<IActionResult> RejectRequest(Guid id, [FromBody] RejectReportRequestDto dto, CancellationToken cancellationToken)
        {
            var request = await _reportRequestService.RejectAsync(id, dto, cancellationToken);
            return Ok(new ApiResponse<ReportRequestResponseDto>(request, "Report request rejected successfully."));
        }

        [HttpGet("requests/{id:guid}/export")]
        public async Task<IActionResult> ExportApprovedRequest(Guid id, [FromQuery] ExportRequestDto exportRequest, CancellationToken cancellationToken)
        {
            var file = await _reportRequestService.ExportAsync(id, exportRequest.Format, cancellationToken);
            return File(file.Content, file.ContentType, file.FileName);
        }
    }
}
