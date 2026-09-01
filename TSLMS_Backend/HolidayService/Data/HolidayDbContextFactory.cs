using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HolidayService.Data
{
    public class HolidayDbContextFactory : IDesignTimeDbContextFactory<HolidayDbContext>
    {
        public HolidayDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<HolidayDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("HolidayDb"));

            return new HolidayDbContext(optionsBuilder.Options);
        }
    }
}
