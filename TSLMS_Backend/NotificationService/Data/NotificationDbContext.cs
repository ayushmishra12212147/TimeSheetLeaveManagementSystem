using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
        public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Message).HasMaxLength(4000).IsRequired();
                entity.Property(x => x.ActionUrl).HasMaxLength(1000);
                entity.Property(x => x.EntityType).HasMaxLength(100);
                entity.Property(x => x.EntityId).HasMaxLength(100);
                entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
                entity.HasIndex(x => new { x.RecipientUserId, x.CreatedAtUtc });
            });

            modelBuilder.Entity<NotificationPreference>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.UserId).IsUnique();
            });

            modelBuilder.Entity<NotificationTemplate>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.EventKey).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.SubjectTemplate).HasMaxLength(500).IsRequired();
                entity.Property(x => x.BodyTemplate).HasMaxLength(8000).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(1000);
                entity.Property(x => x.Channel).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(x => new { x.EventKey, x.Channel }).IsUnique();
            });
        }
    }
}
