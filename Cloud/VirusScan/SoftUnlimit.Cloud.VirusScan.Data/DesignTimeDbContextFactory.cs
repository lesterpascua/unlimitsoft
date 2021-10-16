using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SoftUnlimit.Cloud.VirusScan.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DbContextWrite>
    {
        public const string Scheme = "viruscan";

        /// <summary>
        /// Create db context for EntityFramework command.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public DbContextWrite CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Development.json"))
                .Build();
            var connString = builder.GetConnectionString("Database");

            var options = new DbContextOptionsBuilder<DbContextWrite>()
                .UseSqlServer(connString)
                .Options;

            return new DbContextWrite(options);
        }
    }
}
