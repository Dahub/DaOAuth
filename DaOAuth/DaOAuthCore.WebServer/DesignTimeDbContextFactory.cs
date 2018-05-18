using DaOAuthCore.Dal.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DaOAuthCore.WebServer
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DaOAuthContext>
    {
        public DaOAuthContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var builder = new DbContextOptionsBuilder<DaOAuthContext>();
            var connectionString = configuration.GetConnectionString("DaOAuthConnexionString");
            builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("DaOAuthCore.WebServer"));
            return new DaOAuthContext(builder.Options);
        }
    }
}