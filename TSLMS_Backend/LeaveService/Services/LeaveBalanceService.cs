using LeaveService.Clients;
using LeaveService.Data;
using LeaveService.DTOs;
using LeaveService.Exceptions;
using LeaveService.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveService.Services
{
    public class LeaveBalanceService : ILeaveBalanceService
    {
        private readonly LeaveDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmployeeDirectoryClient _employeeDirectoryClient;

        public LeaveBalanceService(
            LeaveDbContext dbContext,
            ICurrentUserService currentUserService,
            IEmployeeDirectoryClient employeeDirectoryClient)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _employeeDirectoryClient = employeeDirectoryClient;
        }

        public async Task<IReadOnlyCollection<LeaveBalanceResponseDto>> GetMyAsync(int? year, CancellationToken cancellationToken = default)
        {
            var employee = await _employeeDirectoryClient.GetUserAsync(_currentUserService.GetUserId(), cancellationToken);
            return await GetBalancesForEmployeeAsync(employee, year, cancellationToken);
        }

        public async Task<IReadOnlyCollection<LeaveBalanceResponseDto>> GetByEmployeeAsync(string employeeId, int? year, CancellationToken cancellationToken = default)
        {
            var currentRole = _currentUserService.GetRole();
            var currentUserId = _currentUserService.GetUserId();
            var currentEmployeeId = _currentUserService.GetEmployeeId();

            if (!string.Equals(employeeId, currentEmployeeId, StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(currentRole, "HRAdmin", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (string.Equals(currentRole, "Manager", StringComparison.OrdinalIgnoreCase))
                {
                    var directReports = await _employeeDirectoryClient.GetUsersAsync(
                        managerId: currentUserId,
                        employeeId: employeeId,
                        cancellationToken: cancellationToken);

                    if (directReports.Count == 0)
                    {
                        throw new ApiException(StatusCodes.Status403Forbidden, "You can only view leave balances for your direct reports.");
                    }
                }
                else
                {
                    throw new ApiException(StatusCodes.Status403Forbidden, "You are not allowed to view another employee's leave balances.");
                }
            }

            var employee = (await _employeeDirectoryClient.GetUsersAsync(employeeId: employeeId, cancellationToken: cancellationToken))
                .FirstOrDefault();

            if (employee == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Employee not found.");
            }

            return await GetBalancesForEmployeeAsync(employee, year, cancellationToken);
        }

        public async Task<LeaveBalanceResponseDto> AdjustAsync(Guid balanceId, AdjustBalanceDto dto, CancellationToken cancellationToken = default)
        {
            EnsureHrAdmin();

            var balance = await _dbContext.LeaveBalances
                .Include(x => x.LeaveType)
                .FirstOrDefaultAsync(x => x.Id == balanceId, cancellationToken);

            if (balance == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Leave balance not found.");
            }

            var actor = await _employeeDirectoryClient.GetUserAsync(_currentUserService.GetUserId(), cancellationToken);

            balance.ManualAdjustmentDays += dto.Days;
            balance.UpdatedAtUtc = DateTime.UtcNow;

            _dbContext.LeaveBalanceAudits.Add(new LeaveBalanceAudit
            {
                Id = Guid.NewGuid(),
                LeaveBalanceId = balance.Id,
                EmployeeUserId = balance.EmployeeUserId,
                EmployeeId = balance.EmployeeId,
                LeaveTypeId = balance.LeaveTypeId,
                LeaveTypeName = balance.LeaveType.Name,
                DeltaDays = dto.Days,
                Reason = dto.Reason.Trim(),
                AdjustedByUserId = actor.Id,
                AdjustedByName = actor.FullName,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            return MapLeaveBalance(balance);
        }

        public async Task<IReadOnlyCollection<LeaveBalanceResponseDto>> CarryForwardAsync(CarryForwardBalanceDto dto, CancellationToken cancellationToken = default)
        {
            EnsureHrAdmin();

            var sourceBalances = await _dbContext.LeaveBalances
                .Include(x => x.LeaveType)
                .Where(x => x.Year == dto.SourceYear)
                .ToListAsync(cancellationToken);

            var result = new List<LeaveBalance>();

            foreach (var source in sourceBalances)
            {
                if (source.LeaveType.MaxCarryForwardDays <= 0)
                {
                    continue;
                }

                var leftover = source.AllocatedDays + source.CarriedForwardDays + source.ManualAdjustmentDays - source.UsedDays;
                var carryForward = Math.Min(Math.Max(leftover, 0m), source.LeaveType.MaxCarryForwardDays);

                var target = await _dbContext.LeaveBalances
                    .Include(x => x.LeaveType)
                    .FirstOrDefaultAsync(
                        x => x.EmployeeUserId == source.EmployeeUserId &&
                             x.LeaveTypeId == source.LeaveTypeId &&
                             x.Year == dto.TargetYear,
                        cancellationToken);

                if (target == null)
                {
                    target = new LeaveBalance
                    {
                        Id = Guid.NewGuid(),
                        EmployeeUserId = source.EmployeeUserId,
                        EmployeeId = source.EmployeeId,
                        LeaveTypeId = source.LeaveTypeId,
                        LeaveType = source.LeaveType,
                        Year = dto.TargetYear,
                        AllocatedDays = source.LeaveType.DefaultAnnualQuota,
                        CarriedForwardDays = carryForward,
                        ManualAdjustmentDays = 0m,
                        PendingDays = 0m,
                        UsedDays = 0m,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    _dbContext.LeaveBalances.Add(target);
                }
                else
                {
                    target.CarriedForwardDays = carryForward;
                    target.UpdatedAtUtc = DateTime.UtcNow;
                }

                result.Add(target);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return result
                .OrderBy(x => x.EmployeeId)
                .ThenBy(x => x.LeaveType.Name)
                .Select(MapLeaveBalance)
                .ToList();
        }

        private async Task<IReadOnlyCollection<LeaveBalanceResponseDto>> GetBalancesForEmployeeAsync(
            EmployeeDirectoryUserDto employee,
            int? year,
            CancellationToken cancellationToken)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;
            var activeLeaveTypes = await _dbContext.LeaveTypes
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            foreach (var leaveType in activeLeaveTypes)
            {
                var exists = await _dbContext.LeaveBalances.AnyAsync(
                    x => x.EmployeeUserId == employee.Id &&
                         x.LeaveTypeId == leaveType.Id &&
                         x.Year == targetYear,
                    cancellationToken);

                if (!exists)
                {
                    _dbContext.LeaveBalances.Add(new LeaveBalance
                    {
                        Id = Guid.NewGuid(),
                        EmployeeUserId = employee.Id,
                        EmployeeId = employee.EmployeeId,
                        LeaveTypeId = leaveType.Id,
                        LeaveType = leaveType,
                        Year = targetYear,
                        AllocatedDays = leaveType.DefaultAnnualQuota,
                        CarriedForwardDays = 0m,
                        ManualAdjustmentDays = 0m,
                        PendingDays = 0m,
                        UsedDays = 0m,
                        UpdatedAtUtc = DateTime.UtcNow
                    });
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            var balances = await _dbContext.LeaveBalances
                .Include(x => x.LeaveType)
                .Where(x => x.EmployeeUserId == employee.Id && x.Year == targetYear)
                .OrderBy(x => x.LeaveType.Name)
                .ToListAsync(cancellationToken);

            return balances.Select(MapLeaveBalance).ToList();
        }

        private void EnsureHrAdmin()
        {
            if (!string.Equals(_currentUserService.GetRole(), "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Only HRAdmin can manage leave balances.");
            }
        }

        private static LeaveBalanceResponseDto MapLeaveBalance(LeaveBalance balance)
        {
            return new LeaveBalanceResponseDto
            {
                Id = balance.Id,
                EmployeeUserId = balance.EmployeeUserId,
                EmployeeId = balance.EmployeeId,
                LeaveTypeId = balance.LeaveTypeId,
                LeaveTypeName = balance.LeaveType.Name,
                Year = balance.Year,
                AllocatedDays = balance.AllocatedDays,
                CarriedForwardDays = balance.CarriedForwardDays,
                ManualAdjustmentDays = balance.ManualAdjustmentDays,
                PendingDays = balance.PendingDays,
                UsedDays = balance.UsedDays,
                AvailableDays = balance.AllocatedDays + balance.CarriedForwardDays + balance.ManualAdjustmentDays - balance.PendingDays - balance.UsedDays
            };
        }
    }
}
