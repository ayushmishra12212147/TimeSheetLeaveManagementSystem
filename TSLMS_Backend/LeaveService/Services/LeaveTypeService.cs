using LeaveService.Data;
using LeaveService.DTOs;
using LeaveService.Exceptions;
using LeaveService.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveService.Services
{
    public class LeaveTypeService : ILeaveTypeService
    {
        private readonly LeaveDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public LeaveTypeService(LeaveDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        public async Task<IReadOnlyCollection<LeaveTypeResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var leaveTypes = await _dbContext.LeaveTypes
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            return leaveTypes.Select(MapLeaveType).ToList();
        }

        public async Task<LeaveTypeResponseDto> CreateAsync(CreateLeaveTypeDto dto, CancellationToken cancellationToken = default)
        {
            EnsureHrAdmin();
            await EnsureCodeAvailableAsync(dto.Code, null, cancellationToken);

            var now = DateTime.UtcNow;
            var leaveType = new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Code = dto.Code.Trim().ToUpperInvariant(),
                Description = NormalizeOptionalText(dto.Description),
                DefaultAnnualQuota = dto.DefaultAnnualQuota,
                MaxCarryForwardDays = dto.MaxCarryForwardDays,
                RequiresDocument = dto.RequiresDocument,
                IsAutoApprove = dto.IsAutoApprove,
                IsActive = dto.IsActive,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _dbContext.LeaveTypes.Add(leaveType);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return MapLeaveType(leaveType);
        }

        public async Task<LeaveTypeResponseDto> UpdateAsync(Guid id, UpdateLeaveTypeDto dto, CancellationToken cancellationToken = default)
        {
            EnsureHrAdmin();

            var leaveType = await GetLeaveTypeAsync(id, cancellationToken);
            await EnsureCodeAvailableAsync(dto.Code, id, cancellationToken);

            leaveType.Name = dto.Name.Trim();
            leaveType.Code = dto.Code.Trim().ToUpperInvariant();
            leaveType.Description = NormalizeOptionalText(dto.Description);
            leaveType.DefaultAnnualQuota = dto.DefaultAnnualQuota;
            leaveType.MaxCarryForwardDays = dto.MaxCarryForwardDays;
            leaveType.RequiresDocument = dto.RequiresDocument;
            leaveType.IsAutoApprove = dto.IsAutoApprove;
            leaveType.IsActive = dto.IsActive;
            leaveType.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return MapLeaveType(leaveType);
        }

        public async Task<LeaveTypeResponseDto> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            EnsureHrAdmin();

            var leaveType = await GetLeaveTypeAsync(id, cancellationToken);
            leaveType.IsActive = !leaveType.IsActive;
            leaveType.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return MapLeaveType(leaveType);
        }

        private void EnsureHrAdmin()
        {
            if (!string.Equals(_currentUserService.GetRole(), "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Only HRAdmin can manage leave types.");
            }
        }

        private async Task<LeaveType> GetLeaveTypeAsync(Guid id, CancellationToken cancellationToken)
        {
            var leaveType = await _dbContext.LeaveTypes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (leaveType == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Leave type not found.");
            }

            return leaveType;
        }

        private async Task EnsureCodeAvailableAsync(string code, Guid? currentId, CancellationToken cancellationToken)
        {
            var normalizedCode = code.Trim().ToUpperInvariant();
            var exists = await _dbContext.LeaveTypes.AnyAsync(
                x => x.Code == normalizedCode && (!currentId.HasValue || x.Id != currentId.Value),
                cancellationToken);

            if (exists)
            {
                throw new ApiException(StatusCodes.Status409Conflict, $"Leave type code {normalizedCode} already exists.");
            }
        }

        private static string? NormalizeOptionalText(string? text)
        {
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }

        private static LeaveTypeResponseDto MapLeaveType(LeaveType leaveType)
        {
            return new LeaveTypeResponseDto
            {
                Id = leaveType.Id,
                Name = leaveType.Name,
                Code = leaveType.Code,
                Description = leaveType.Description,
                DefaultAnnualQuota = leaveType.DefaultAnnualQuota,
                MaxCarryForwardDays = leaveType.MaxCarryForwardDays,
                RequiresDocument = leaveType.RequiresDocument,
                IsAutoApprove = leaveType.IsAutoApprove,
                IsActive = leaveType.IsActive,
                UpdatedAtUtc = leaveType.UpdatedAtUtc
            };
        }
    }
}
