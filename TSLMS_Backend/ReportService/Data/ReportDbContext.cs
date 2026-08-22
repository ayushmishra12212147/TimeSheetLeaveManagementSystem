using Microsoft.EntityFrameworkCore;
using ReportService.Models;

namespace ReportService.Data
{
    public class ReportDbContext : DbContext
    {
        public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options)
        {
        }

        public DbSet<ReportRequest> ReportRequests => Set<ReportRequest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReportRequest>(entity =>
            {
                entity.ToTable("ReportRequests");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.RequestedByEmployeeId).HasMaxLength(20).IsRequired();
                entity.Property(x => x.RequestedByName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.ScopeEmployeeId).HasMaxLength(20);
                entity.Property(x => x.ApprovedByName).HasMaxLength(200);
                entity.Property(x => x.RejectedByName).HasMaxLength(200);
                entity.Property(x => x.RejectionReason).HasMaxLength(1000);
                entity.HasIndex(x => new { x.Status, x.CreatedAtUtc });
                entity.HasIndex(x => new { x.RequestedByUserId, x.CreatedAtUtc });
            });
        }
    }
}
