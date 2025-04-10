using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace Infrastructure.Data
{
    public class ElectricityDbContextFactory : IDesignTimeDbContextFactory<ElectricityDbContext>
    {
        public ElectricityDbContext CreateDbContext(string[] args)
        {
            // Load configuration from appsettings.json
            var basePath = Directory.GetCurrentDirectory();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath) // Use the base path variable
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ElectricityDbContext>();

            // Get connection string from configuration
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Configure MySQL
            optionsBuilder.UseMySql(connectionString,
                ServerVersion.AutoDetect(connectionString));

            return new ElectricityDbContext(optionsBuilder.Options);
        }
    }
}