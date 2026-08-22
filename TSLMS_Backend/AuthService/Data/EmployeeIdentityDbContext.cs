using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class EmployeeIdentityDbContext : DbContext
    {
        public EmployeeIdentityDbContext(DbContextOptions<EmployeeIdentityDbContext> options)
            : base(options)
        {
        }

        public DbSet<EmployeeUser> Users => Set<EmployeeUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeUser>()
                .ToTable("Users")
                .HasKey(x => x.Id);
        }
    }
}
