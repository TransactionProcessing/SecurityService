using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SecurityService.Database.DbContexts;

namespace SecurityService.SqlServerMigrations;

public sealed class SecurityServiceDbContextFactory : IDesignTimeDbContextFactory<SecurityServiceDbContext>
{
    public SecurityServiceDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("NEW_SECURITYSERVICE_CONNECTIONSTRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Server=(localdb)\\mssqllocaldb;Database=NewSecurityService;Trusted_Connection=True;MultipleActiveResultSets=true";
        }

        var optionsBuilder = new DbContextOptionsBuilder<SecurityServiceDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlServerOptions =>
        {
            sqlServerOptions.MigrationsAssembly(typeof(SecurityServiceDbContextFactory).Assembly.GetName().Name);
        });
        optionsBuilder.UseOpenIddict();

        return new SecurityServiceDbContext(optionsBuilder.Options);
    }
}
