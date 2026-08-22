using EmployeeService.DTOs;
using EmployeeService.Models;

namespace EmployeeService.Services
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAllAsync();
        Task<Department> GetByIdAsync(Guid id);
        Task<Department> CreateAsync(Department department);
        Task<Department> UpdateAsync(Guid id, string name);
        Task<bool> DeleteAsync(Guid id);
    }
}