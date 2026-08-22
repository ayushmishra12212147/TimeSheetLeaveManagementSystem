using AuditService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuditService.Data
{
    public class AuditDbContext : DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options)
            : base(options)
        {
        }

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            EnsureAppendOnly();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            EnsureAppendOnly();
            return base.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLog>().HasIndex(x => x.OccurredAtUtc);
            modelBuilder.Entity<AuditLog>().HasIndex(x => x.ServiceName);
            modelBuilder.Entity<AuditLog>().HasIndex(x => x.EventKey);
            modelBuilder.Entity<AuditLog>().Property(x => x.MetadataJson).HasColumnType("nvarchar(max)");
        }

        private void EnsureAppendOnly()
        {
            var invalidEntry = ChangeTracker.Entries<AuditLog>()
                .FirstOrDefault(x => x.State is EntityState.Modified or EntityState.Deleted);

            if (invalidEntry != null)
            {
                throw new InvalidOperationException("Audit logs are append-only.");
            }
        }
    }
}
