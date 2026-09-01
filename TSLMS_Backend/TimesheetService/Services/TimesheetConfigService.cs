using Microsoft.EntityFrameworkCore;
using TimesheetService.Data;
using TimesheetService.DTOs;
using TimesheetService.Exceptions;
using TimesheetService.Models;

namespace TimesheetService.Services
{
    public class TimesheetConfigService : ITimesheetConfigService
    {
        private readonly TimesheetDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public TimesheetConfigService(TimesheetDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        public async Task<TimesheetConfigResponseDto> GetAsync(CancellationToken cancellationToken = default)
        {
            var config = await GetOrCreateConfigAsync(cancellationToken);
            return MapConfig(config);
        }

        public async Task<TimesheetConfigResponseDto> UpdateAsync(UpdateTimesheetConfigDto dto, CancellationToken cancellationToken = default)
        {
            EnsureHrAdmin();

            var config = await GetOrCreateConfigAsync(cancellationToken);
            config.MinimumWeeklyHours = dto.MinimumWeeklyHours;
            config.LowHoursWarningThreshold = dto.LowHoursWarningThreshold;
            config.HighHoursWarningThreshold = dto.HighHoursWarningThreshold;
            config.AutoApproveEnabled = dto.AutoApproveEnabled;
            config.AutoApproveAfterHours = dto.AutoApproveAfterHours;
            config.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return MapConfig(config);
        }

        private void EnsureHrAdmin()
        {
            if (!string.Equals(_currentUserService.GetRole(), "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Only HRAdmin can manage timesheet config.");
            }
        }

        private async Task<TimesheetAutoApproveConfig> GetOrCreateConfigAsync(CancellationToken cancellationToken)
        {
            var config = await _dbContext.TimesheetAutoApproveConfigs.FirstOrDefaultAsync(cancellationToken);
            if (config != null)
            {
                return config;
            }

            config = new TimesheetAutoApproveConfig
            {
                Id = Guid.NewGuid(),
                MinimumWeeklyHours = 40m,
                LowHoursWarningThreshold = 8m,
                HighHoursWarningThreshold = 12m,
                AutoApproveEnabled = false,
                AutoApproveAfterHours = 48,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _dbContext.TimesheetAutoApproveConfigs.Add(config);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return config;
        }

        private static TimesheetConfigResponseDto MapConfig(TimesheetAutoApproveConfig config)
        {
            return new TimesheetConfigResponseDto
            {
                Id = config.Id,
                MinimumWeeklyHours = config.MinimumWeeklyHours,
                LowHoursWarningThreshold = config.LowHoursWarningThreshold,
                HighHoursWarningThreshold = config.HighHoursWarningThreshold,
                AutoApproveEnabled = config.AutoApproveEnabled,
                AutoApproveAfterHours = config.AutoApproveAfterHours,
                UpdatedAtUtc = config.UpdatedAtUtc
            };
        }
    }
}
