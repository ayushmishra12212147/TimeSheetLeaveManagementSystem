using HolidayService.Models;
using Microsoft.EntityFrameworkCore;

namespace HolidayService.Data
{
    public class HolidayDbContext : DbContext
    {
        public HolidayDbContext(DbContextOptions<HolidayDbContext> options)
            : base(options)
        {
        }

        public DbSet<Holiday> Holidays => Set<Holiday>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Holiday>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(500);
                entity.Property(x => x.HolidayDate).HasColumnType("date");
                entity.HasIndex(x => x.HolidayDate).IsUnique();
            });
        }
    }
}
