using EmployeeService.Data;
using EmployeeService.DTOs;
using EmployeeService.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly AppDbContext _context;

        public DepartmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentDto>> GetAllAsync()
        {
            var departments = await _context.Departments
        .Include(d => d.Users)
        .ToListAsync();

            return departments.Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Users = d.Users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    EmployeeId = u.EmployeeId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role

                }).ToList()
            }).ToList();
        }

        public async Task<Department> GetByIdAsync(Guid id)
        {
            return await _context.Departments.FindAsync(id);
        }

        public async Task<Department> CreateAsync(Department department)
        {
            department.Id = Guid.NewGuid();
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return department;
        }
    
        public async Task<Department> UpdateAsync(Guid id, string name)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return null;
    
            dept.Name = name;
            await _context.SaveChangesAsync();
            return dept;
        }
    
        public async Task<bool> DeleteAsync(Guid id)
        {
            var dept = await _context.Departments
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.Id == id);
    
            if (dept == null) return false;
    
            // Prevent deletion if department has users
            if (dept.Users.Any())
            {
                throw new InvalidOperationException("Cannot delete a department that has active users.");
            }
    
            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
