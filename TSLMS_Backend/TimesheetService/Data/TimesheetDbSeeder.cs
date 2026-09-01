using Microsoft.EntityFrameworkCore;
using TimesheetService.Models;

namespace TimesheetService.Data
{
    public static class TimesheetDbSeeder
    {
        public static async Task SeedAsync(TimesheetDbContext dbContext)
        {
            var exists = await dbContext.TimesheetAutoApproveConfigs.AnyAsync();
            if (exists)
            {
                return;
            }

            dbContext.TimesheetAutoApproveConfigs.Add(new TimesheetAutoApproveConfig
            {
                Id = Guid.Parse("3389ef56-2526-4de5-9329-a1f1a39d1001"),
                MinimumWeeklyHours = 40m,
                LowHoursWarningThreshold = 8m,
                HighHoursWarningThreshold = 12m,
                AutoApproveEnabled = false,
                AutoApproveAfterHours = 48,
                UpdatedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }
    }
}
