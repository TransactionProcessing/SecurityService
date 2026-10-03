using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic;
using SecurityService.Database.DbContexts;
using SecurityService.HostedServices;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;

namespace SecurityService.UnitTests.HostedServices;

public sealed class ManagementBootstrapperTests
{
    [Fact]
    public async Task InitializeAsync_WhenEnabled_CreatesBootstrapClientAndIsIdempotent()
    {
        using var provider = TestServiceProviderFactory.Create(
            Guid.NewGuid().ToString(),
            configureOptions: options =>
            {
                options.ManagementBootstrap.Enabled = true;
                options.ManagementBootstrap.ClientId = "bootstrap-client";
                options.ManagementBootstrap.ClientSecret = "bootstrap-secret";
            });

        using var scope = provider.CreateScope();
        var bootstrapper = new ManagementBootstrapper(
            scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>(),
            scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>(),
            scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<SecurityService.Database.Entities.ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>(),
            scope.ServiceProvider.GetRequiredService<IOptions<ServiceOptions>>());

        await bootstrapper.InitializeAsync(CancellationToken.None);
        await bootstrapper.InitializeAsync(CancellationToken.None);

        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        (await applicationManager.FindByClientIdAsync("bootstrap-client")).ShouldNotBeNull();
        (await dbContext.ClientDefinitions.CountAsync(client => client.ClientId == "bootstrap-client")).ShouldBe(1);
    }
}
