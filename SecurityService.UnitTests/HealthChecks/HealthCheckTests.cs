using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SecurityService.BusinessLogic;
using SecurityService.Database.DbContexts;
using SecurityService.HealthChecks;
using Shouldly;

namespace SecurityService.UnitTests.HealthChecks;

public class HealthCheckTests
{
    [Fact]
    public async Task DatabaseHealthCheck_InMemoryDatabase_IsHealthy()
    {
        var dbContext = new SecurityServiceDbContext(new DbContextOptionsBuilder<SecurityServiceDbContext>()
            .UseInMemoryDatabase(nameof(this.DatabaseHealthCheck_InMemoryDatabase_IsHealthy))
            .Options);
        var healthCheck = new DatabaseHealthCheck(dbContext);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task IssuerHealthCheck_InvalidIssuer_IsUnhealthy()
    {
        var healthCheck = new IssuerHealthCheck(Options.Create(new ServiceOptions()
        {
            IssuerUrl = "not a uri"
        }));

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Unhealthy);
    }
}
