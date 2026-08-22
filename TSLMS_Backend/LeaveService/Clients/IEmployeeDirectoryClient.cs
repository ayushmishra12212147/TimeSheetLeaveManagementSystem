using LeaveService.DTOs;

namespace LeaveService.Clients
{
    public interface IEmployeeDirectoryClient
    {
        Task<EmployeeDirectoryUserDto> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<EmployeeDirectoryUserDto>> GetUsersAsync(string? role = null, Guid? managerId = null, string? employeeId = null, CancellationToken cancellationToken = default);
    }
}
