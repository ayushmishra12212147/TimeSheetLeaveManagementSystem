using LeaveService.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveService.Data
{
    public class LeaveDbContext : DbContext
    {
        public LeaveDbContext(DbContextOptions<LeaveDbContext> options)
            : base(options)
        {
        }

        public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
        public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
        public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
        public DbSet<LeaveBalanceAudit> LeaveBalanceAudits => Set<LeaveBalanceAudit>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LeaveType>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(20).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(500);
                entity.Property(x => x.DefaultAnnualQuota).HasColumnType("decimal(5,2)");
                entity.Property(x => x.MaxCarryForwardDays).HasColumnType("decimal(5,2)");
                entity.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<LeaveBalance>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.EmployeeId).HasMaxLength(20).IsRequired();
                entity.Property(x => x.AllocatedDays).HasColumnType("decimal(6,2)");
                entity.Property(x => x.CarriedForwardDays).HasColumnType("decimal(6,2)");
                entity.Property(x => x.ManualAdjustmentDays).HasColumnType("decimal(6,2)");
                entity.Property(x => x.PendingDays).HasColumnType("decimal(6,2)");
                entity.Property(x => x.UsedDays).HasColumnType("decimal(6,2)");
                entity.HasIndex(x => new { x.EmployeeUserId, x.LeaveTypeId, x.Year }).IsUnique();
                entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<LeaveBalanceAudit>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.EmployeeId).HasMaxLength(20).IsRequired();
                entity.Property(x => x.LeaveTypeName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.DeltaDays).HasColumnType("decimal(6,2)");
                entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
                entity.HasOne(x => x.LeaveBalance).WithMany().HasForeignKey(x => x.LeaveBalanceId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.EmployeeId).HasMaxLength(20).IsRequired();
                entity.Property(x => x.EmployeeName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.EmployeeEmail).HasMaxLength(256).IsRequired();
                entity.Property(x => x.ManagerName).HasMaxLength(100);
                entity.Property(x => x.ManagerEmail).HasMaxLength(256);
                entity.Property(x => x.LeaveTypeName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.StartDate).HasColumnType("date");
                entity.Property(x => x.EndDate).HasColumnType("date");
                entity.Property(x => x.RequestedDays).HasColumnType("decimal(6,2)");
                entity.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
                entity.Property(x => x.SupportingDocumentUrl).HasMaxLength(1000);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
                entity.Property(x => x.PendingApprovalRole).HasConversion<string>().HasMaxLength(20);
                entity.Property(x => x.ApprovedByName).HasMaxLength(100);
                entity.Property(x => x.RejectedByName).HasMaxLength(100);
                entity.Property(x => x.RejectionReason).HasMaxLength(1000);
                entity.Property(x => x.CancelledByName).HasMaxLength(100);
                entity.Property(x => x.CancellationReason).HasMaxLength(1000);
                entity.HasIndex(x => new { x.EmployeeUserId, x.StartDate, x.EndDate });
                entity.HasIndex(x => new { x.ManagerUserId, x.Status });
                entity.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
