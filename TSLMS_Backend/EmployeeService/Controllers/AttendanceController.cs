using EmployeeService.DTOs;
using EmployeeService.Helpers;
using EmployeeService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/attendance")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPost("generate-qr")]
        public async Task<IActionResult> GenerateQr([FromBody] GenerateQrDto dto, CancellationToken cancellationToken)
        {
            var qr = await _attendanceService.GenerateQrAsync(dto, cancellationToken);
            return Ok(new ApiResponse<GenerateQrResponseDto>(qr, "Attendance QR generated successfully."));
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("scan-in")]
        public async Task<IActionResult> ScanIn([FromBody] ScanQrDto dto, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.ScanInAsync(dto, cancellationToken);
            return Ok(new ApiResponse<AttendanceResponseDto>(result, "Clock-in recorded successfully."));
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("scan-out")]
        public async Task<IActionResult> ScanOut([FromBody] ScanQrDto dto, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.ScanOutAsync(dto, cancellationToken);
            return Ok(new ApiResponse<AttendanceResponseDto>(result, "Clock-out recorded successfully."));
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy([FromQuery] DateOnly? dateFrom, [FromQuery] DateOnly? dateTo, CancellationToken cancellationToken)
        {
            var records = await _attendanceService.GetMyAsync(dateFrom, dateTo, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<AttendanceResponseDto>>(records, "Attendance history fetched successfully."));
        }

        [Authorize(Roles = "Manager,HRAdmin")]
        [HttpGet("team")]
        public async Task<IActionResult> GetTeam([FromQuery] DateOnly? date, CancellationToken cancellationToken)
        {
            var records = await _attendanceService.GetTeamAsync(date, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<AttendanceResponseDto>>(records, "Team attendance fetched successfully."));
        }

        [Authorize(Roles = "Manager,HRAdmin")]
        [HttpGet("{employeeUserId:guid}/history")]
        public async Task<IActionResult> GetHistory(Guid employeeUserId, [FromQuery] DateOnly? dateFrom, [FromQuery] DateOnly? dateTo, CancellationToken cancellationToken)
        {
            var records = await _attendanceService.GetHistoryAsync(employeeUserId, dateFrom, dateTo, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<AttendanceResponseDto>>(records, "Attendance history fetched successfully."));
        }

        [Authorize(Roles = "Manager,HRAdmin")]
        [HttpGet("records")]
        public async Task<IActionResult> GetRecords([FromQuery] DateOnly? dateFrom, [FromQuery] DateOnly? dateTo, [FromQuery] string? employeeId, CancellationToken cancellationToken)
        {
            var records = await _attendanceService.GetReportAsync(dateFrom, dateTo, employeeId, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<AttendanceResponseDto>>(records, "Attendance records fetched successfully."));
        }
    }
}
