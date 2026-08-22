using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TimesheetService.Data
{
    public class TimesheetDbContextFactory : IDesignTimeDbContextFactory<TimesheetDbContext>
    {
        public TimesheetDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<TimesheetDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("TimesheetDb"));

            return new TimesheetDbContext(optionsBuilder.Options);
        }
    }
}
