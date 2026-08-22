using BCrypt.Net;
using EmployeeService.Data;
using EmployeeService.DTOs;
using EmployeeService.Helpers;
using EmployeeService.Messaging;
using EmployeeService.Models;
using Microsoft.EntityFrameworkCore;
using EmployeeService.Events;

namespace EmployeeService.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IRabbitMQPublisher _publisher;

        public UserService(AppDbContext context, IRabbitMQPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }

        public async Task<List<UserResponseDto>> GetAllAsync(string? role = null, Guid? managerId = null, string? employeeId = null)
        {
            var query = _context.Users
                .Include(u => u.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role == role);
            }

            if (managerId.HasValue)
            {
                query = query.Where(u => u.ManagerId == managerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                query = query.Where(u => u.EmployeeId == employeeId);
            }

            var users = await query.ToListAsync();

            return users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                EmployeeId = u.EmployeeId,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                Gender = u.Gender,
                DepartmentId = u.DepartmentId,
                ManagerId = u.ManagerId,
                Department = u.Department == null ? null : new DepartmentDto
                {
                    Id = u.Department.Id,
                    Name = u.Department.Name
                }
            }).ToList();
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _context.Users.Include(u => u.Department)
                                       .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<UserResponseDto> CreateAsync(UserDto dto)
        {
            // 1. Generate temp password
            var tempPassword = PasswordGenerator.Generate(12);

            // 2. Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(tempPassword);
            var employeeId = await GenerateEmployeeIdAsync(dto.Role);

            // 3. Create entity
            var user = new User
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role,
                Gender = dto.Gender,
                DepartmentId = dto.DepartmentId,
                Password = hashedPassword,
                IsFirstLogin = true,
                MustResetPassword = true,
                IsProfileComplete = false,
                TempPasswordExpiry = null
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 4. Publish event
            var userEvent = new UserCreatedEvent
            {
                UserId = user.Id,
                EmployeeId = user.EmployeeId,
                Email = user.Email,
                FullName = user.FullName,
                TempPassword = tempPassword
            };

            _publisher.Publish(userEvent, "user.created");

            // 5. Return clean DTO
            return new UserResponseDto
            {
                Id = user.Id,
                EmployeeId = user.EmployeeId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Gender = user.Gender,
                DepartmentId = user.DepartmentId,
                ManagerId = user.ManagerId,
                Department = null
            };
        }

        public async Task<User> UpdateAsync(Guid id, User user)
        {
            var existing = await _context.Users.FindAsync(id);
            if (existing == null) return null;

            existing.FullName = user.FullName;
            existing.Email = user.Email;
            existing.Role = user.Role;
            existing.Gender = user.Gender;
            existing.DepartmentId = user.DepartmentId;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignManagerAsync(Guid userId, Guid managerId)
        {
            var user = await _context.Users.FindAsync(userId);
            var manager = await _context.Users.FindAsync(managerId);

            if (user == null || manager == null)
                return false;

            var previousManager = user.ManagerId.HasValue
                ? await _context.Users.FindAsync(user.ManagerId.Value)
                : null;

            user.ManagerId = managerId;

            await _context.SaveChangesAsync();

            _publisher.Publish(new ManagerAssignmentChangedEvent
            {
                EmployeeUserId = user.Id,
                EmployeeId = user.EmployeeId,
                EmployeeName = user.FullName,
                PreviousManagerUserId = previousManager?.Id,
                PreviousManagerName = previousManager?.FullName,
                PreviousManagerEmail = previousManager?.Email,
                CurrentManagerUserId = manager.Id,
                CurrentManagerName = manager.FullName,
                CurrentManagerEmail = manager.Email,
                Action = previousManager == null ? "Assigned" : "Reassigned",
                ChangedAtUtc = DateTime.UtcNow
            }, "user.manager-assignment.changed");

            return true;
        }

        private async Task<string> GenerateEmployeeIdAsync(string role)
        {
            var prefix = GetPrefix(role);

            var existingIds = await _context.Users
                .Where(u => u.EmployeeId.StartsWith(prefix))
                .Select(u => u.EmployeeId)
                .ToListAsync();

            var nextNumber = existingIds
                .Select(ParseSequence)
                .DefaultIfEmpty(26000)
                .Max() + 1;

            return $"{prefix}{nextNumber:D5}";
        }

        private static string GetPrefix(string role)
        {
            return role switch
            {
                "Manager" => "MGR",
                "HRAdmin" => "ADM",
                _ => "EMP"
            };
        }

        private static int ParseSequence(string employeeId)
        {
            if (employeeId.Length <= 3)
            {
                return 26000;
            }

            return int.TryParse(employeeId[3..], out var sequence)
                ? sequence
                : 26000;
        }
    }
}
