using EmployeeService.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Data
{
    public class AttendanceDbContext : DbContext
    {
        public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
            : base(options)
        {
        }

        public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttendanceRecord>()
                .HasIndex(x => new { x.EmployeeUserId, x.AttendanceDate })
                .IsUnique();

            modelBuilder.Entity<AttendanceRecord>()
                .Property(x => x.Status)
                .HasConversion<int>();
        }
    }
}
