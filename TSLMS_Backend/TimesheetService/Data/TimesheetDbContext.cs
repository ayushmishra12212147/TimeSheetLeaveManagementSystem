using Microsoft.EntityFrameworkCore;
using TimesheetService.Models;

namespace TimesheetService.Data
{
    public class TimesheetDbContext : DbContext
    {
        public TimesheetDbContext(DbContextOptions<TimesheetDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<TimesheetEntry> TimesheetEntries => Set<TimesheetEntry>();
        public DbSet<WeeklyTimesheetSummary> WeeklyTimesheetSummaries => Set<WeeklyTimesheetSummary>();
        public DbSet<TimesheetAutoApproveConfig> TimesheetAutoApproveConfigs => Set<TimesheetAutoApproveConfig>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(20).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(500);
                entity.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<TimesheetAutoApproveConfig>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.MinimumWeeklyHours).HasColumnType("decimal(6,2)");
                entity.Property(x => x.LowHoursWarningThreshold).HasColumnType("decimal(6,2)");
                entity.Property(x => x.HighHoursWarningThreshold).HasColumnType("decimal(6,2)");
            });

            modelBuilder.Entity<WeeklyTimesheetSummary>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.EmployeeId).HasMaxLength(20).IsRequired();
                entity.Property(x => x.EmployeeName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.EmployeeEmail).HasMaxLength(256).IsRequired();
                entity.Property(x => x.ManagerName).HasMaxLength(100);
                entity.Property(x => x.ManagerEmail).HasMaxLength(256);
                entity.Property(x => x.WeekStartDate).HasColumnType("date");
                entity.Property(x => x.WeekEndDate).HasColumnType("date");
                entity.Property(x => x.TotalHours).HasColumnType("decimal(6,2)");
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(x => x.ApprovedByName).HasMaxLength(100);
                entity.Property(x => x.RejectedByName).HasMaxLength(100);
                entity.Property(x => x.RejectionReason).HasMaxLength(1000);
                entity.HasIndex(x => new { x.EmployeeUserId, x.WeekStartDate }).IsUnique();
                entity.HasIndex(x => new { x.ManagerUserId, x.Status });
            });

            modelBuilder.Entity<TimesheetEntry>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.EntryDate).HasColumnType("date");
                entity.Property(x => x.ProjectName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Hours).HasColumnType("decimal(5,2)");
                entity.Property(x => x.Description).HasMaxLength(1000);
                entity.Property(x => x.Category).HasConversion<string>().HasMaxLength(20);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(x => new { x.WeeklyTimesheetSummaryId, x.EntryDate });
                entity.HasOne(x => x.WeeklyTimesheetSummary)
                    .WithMany(x => x.Entries)
                    .HasForeignKey(x => x.WeeklyTimesheetSummaryId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Project)
                    .WithMany()
                    .HasForeignKey(x => x.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
