using LeaveService.DTOs;
using LeaveService.Helpers;
using LeaveService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/leave-types")]
    public class LeaveTypesController : ControllerBase
    {
        private readonly ILeaveTypeService _leaveTypeService;

        public LeaveTypesController(ILeaveTypeService leaveTypeService)
        {
            _leaveTypeService = leaveTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var leaveTypes = await _leaveTypeService.GetAllAsync(cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<LeaveTypeResponseDto>>(leaveTypes, "Leave types fetched successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLeaveTypeDto dto, CancellationToken cancellationToken)
        {
            var leaveType = await _leaveTypeService.CreateAsync(dto, cancellationToken);
            return Ok(new ApiResponse<LeaveTypeResponseDto>(leaveType, "Leave type created successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveTypeDto dto, CancellationToken cancellationToken)
        {
            var leaveType = await _leaveTypeService.UpdateAsync(id, dto, cancellationToken);
            return Ok(new ApiResponse<LeaveTypeResponseDto>(leaveType, "Leave type updated successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPost("{id:guid}/toggle")]
        public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
        {
            var leaveType = await _leaveTypeService.ToggleActiveAsync(id, cancellationToken);
            return Ok(new ApiResponse<LeaveTypeResponseDto>(leaveType, "Leave type status updated successfully."));
        }
    }
}
