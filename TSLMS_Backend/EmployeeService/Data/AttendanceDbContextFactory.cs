using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EmployeeService.Data
{
    public class AttendanceDbContextFactory : IDesignTimeDbContextFactory<AttendanceDbContext>
    {
        public AttendanceDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AttendanceDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("AttendanceDb"));

            return new AttendanceDbContext(optionsBuilder.Options);
        }
    }
}
