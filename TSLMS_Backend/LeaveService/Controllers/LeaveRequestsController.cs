using LeaveService.DTOs;
using LeaveService.Helpers;
using LeaveService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/leaves")]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;

        public LeaveRequestsController(ILeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? employeeId, CancellationToken cancellationToken)
        {
            var requests = await _leaveRequestService.GetVisibleAsync(employeeId, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<LeaveRequestResponseDto>>(requests, "Leave requests fetched successfully."));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var request = await _leaveRequestService.GetByIdAsync(id, cancellationToken);
            return Ok(new ApiResponse<LeaveRequestResponseDto>(request, "Leave request fetched successfully."));
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
        {
            var requests = await _leaveRequestService.GetPendingAsync(cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<LeaveRequestResponseDto>>(requests, "Pending leave requests fetched successfully."));
        }

        [HttpGet("team-calendar")]
        public async Task<IActionResult> GetTeamCalendar(
            [FromQuery] DateOnly? dateFrom,
            [FromQuery] DateOnly? dateTo,
            CancellationToken cancellationToken)
        {
            var requests = await _leaveRequestService.GetTeamCalendarAsync(dateFrom, dateTo, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<LeaveRequestResponseDto>>(requests, "Team calendar fetched successfully."));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto dto, CancellationToken cancellationToken)
        {
            var request = await _leaveRequestService.CreateAsync(dto, cancellationToken);
            return Ok(new ApiResponse<LeaveRequestResponseDto>(request, "Leave request submitted successfully."));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveRequestDto dto, CancellationToken cancellationToken)
        {
            var request = await _leaveRequestService.UpdateAsync(id, dto, cancellationToken);
            return Ok(new ApiResponse<LeaveRequestResponseDto>(request, "Leave request updated successfully."));
        }

        [HttpPatch("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveLeaveDto dto, CancellationToken cancellationToken)
        {
            var request = await _leaveRequestService.ApproveAsync(id, dto, cancellationToken);
            return Ok(new ApiResponse<LeaveRequestResponseDto>(request, "Leave request approved successfully."));
        }

        [HttpPatch("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLeaveDto dto, CancellationToken cancellationToken)
        {
            var request = await _leaveRequestService.RejectAsync(id, dto, cancellationToken);
            return Ok(new ApiResponse<LeaveRequestResponseDto>(request, "Leave request rejected successfully."));
        }

        [HttpPatch("{id:guid}/withdraw")]
        public async Task<IActionResult> Withdraw(Guid id, CancellationToken cancellationToken)
        {
            var request = await _leaveRequestService.WithdrawAsync(id, cancellationToken);
            return Ok(new ApiResponse<LeaveRequestResponseDto>(request, "Leave request withdrawn successfully."));
        }

        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            var request = await _leaveRequestService.CancelAsync(id, cancellationToken);
            return Ok(new ApiResponse<LeaveRequestResponseDto>(request, "Leave request cancelled successfully."));
        }
    }
}
