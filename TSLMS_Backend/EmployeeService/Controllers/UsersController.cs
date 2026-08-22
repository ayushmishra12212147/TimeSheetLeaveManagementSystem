using EmployeeService.DTOs;
using EmployeeService.Helpers;
using EmployeeService.Models;
using EmployeeService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeService.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? role = null, [FromQuery] Guid? managerId = null, [FromQuery] string? employeeId = null)
        {
            var users = await _service.GetAllAsync(role, managerId, employeeId);


            return Ok(new ApiResponse<object>(users, "Users fetched"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var user = await _service.GetByIdAsync(id);

            if (user == null)
                return NotFound(new ApiResponse<string>("User not found"));

            var response = new UserResponseDto
            {
                Id = user.Id,
                EmployeeId = user.EmployeeId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId,
                ManagerId = user.ManagerId,
                Department = user.Department == null ? null : new DepartmentDto
                {
                    Id = user.Department.Id,
                    Name = user.Department.Name
                }
            };

            return Ok(new ApiResponse<object>(response, "User fetched"));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromHeader(Name = "X-User-Role")] string role, [FromBody] UserDto dto)
        {
            if (!string.Equals(role, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new ApiResponse<string>("Only HRAdmin can create users"));
            }

            var result = await _service.CreateAsync(dto);

            return Ok(new ApiResponse<object>(result, "User created"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UserDto dto)
        {
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role,
                DepartmentId = dto.DepartmentId
            };

            var result = await _service.UpdateAsync(id, user);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new ApiResponse<string>("User not found"));

            return Ok(new ApiResponse<string>("User deleted"));
        }

        [HttpPost("assign-manager")]
        public async Task<IActionResult> AssignManager([FromBody] AssignManagerDto dto)
        {
            var result = await _service.AssignManagerAsync(dto.UserId, dto.ManagerId);

            if (!result)
                return NotFound(new ApiResponse<string>("User or Manager not found"));

            return Ok(new ApiResponse<string>("Manager assigned successfully"));
        }
    }
}
