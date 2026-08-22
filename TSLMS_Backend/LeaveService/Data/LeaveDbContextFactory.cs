using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LeaveService.Data
{
    public class LeaveDbContextFactory : IDesignTimeDbContextFactory<LeaveDbContext>
    {
        public LeaveDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<LeaveDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("LeaveDb"));

            return new LeaveDbContext(optionsBuilder.Options);
        }
    }
}
