using EmployeeService.DTOs;
using EmployeeService.Models;

namespace EmployeeService.Services
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> GetAllAsync(string? role = null, Guid? managerId = null, string? employeeId = null);
        Task<User> GetByIdAsync(Guid id);
        Task<UserResponseDto> CreateAsync(UserDto dto);
        Task<User> UpdateAsync(Guid id, User user);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> AssignManagerAsync(Guid userId, Guid managerId);
    }
}
