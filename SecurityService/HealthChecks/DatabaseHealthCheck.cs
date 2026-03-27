using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SecurityService.Database.DbContexts;

namespace SecurityService.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly SecurityServiceDbContext _dbContext;

    public DatabaseHealthCheck(SecurityServiceDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (this._dbContext.Database.IsInMemory())
        {
            return HealthCheckResult.Healthy("The in-memory database is available.");
        }

        return await this._dbContext.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy("The database connection is healthy.")
            : HealthCheckResult.Unhealthy("The database connection could not be established.");
    }
}
