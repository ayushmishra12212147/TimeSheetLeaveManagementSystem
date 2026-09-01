using LeaveService.DTOs;
using LeaveService.Helpers;
using LeaveService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/leave-balances")]
    public class LeaveBalancesController : ControllerBase
    {
        private readonly ILeaveBalanceService _leaveBalanceService;

        public LeaveBalancesController(ILeaveBalanceService leaveBalanceService)
        {
            _leaveBalanceService = leaveBalanceService;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy([FromQuery] int? year, CancellationToken cancellationToken)
        {
            var balances = await _leaveBalanceService.GetMyAsync(year, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<LeaveBalanceResponseDto>>(balances, "Leave balances fetched successfully."));
        }

        [HttpGet("{employeeId}")]
        public async Task<IActionResult> GetByEmployee(string employeeId, [FromQuery] int? year, CancellationToken cancellationToken)
        {
            var balances = await _leaveBalanceService.GetByEmployeeAsync(employeeId, year, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<LeaveBalanceResponseDto>>(balances, "Leave balances fetched successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPatch("{id:guid}/adjust")]
        public async Task<IActionResult> Adjust(Guid id, [FromBody] AdjustBalanceDto dto, CancellationToken cancellationToken)
        {
            var balance = await _leaveBalanceService.AdjustAsync(id, dto, cancellationToken);
            return Ok(new ApiResponse<LeaveBalanceResponseDto>(balance, "Leave balance adjusted successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPost("carry-forward")]
        public async Task<IActionResult> CarryForward([FromBody] CarryForwardBalanceDto dto, CancellationToken cancellationToken)
        {
            var balances = await _leaveBalanceService.CarryForwardAsync(dto, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<LeaveBalanceResponseDto>>(balances, "Carry forward completed successfully."));
        }
    }
}
