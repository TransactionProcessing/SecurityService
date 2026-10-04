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

    [Fact]
    public async Task InitializeAsync_WhenBootstrapApplicationSecretConflicts_FailsClearly()
    {
        using var provider = TestServiceProviderFactory.Create(
            Guid.NewGuid().ToString(),
            configureOptions: options =>
            {
                options.ManagementBootstrap.Enabled = true;
                options.ManagementBootstrap.ClientId = "bootstrap-client";
                options.ManagementBootstrap.ClientSecret = "configured-secret";
            });

        using var scope = provider.CreateScope();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "bootstrap-client",
            ClientSecret = "existing-secret",
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit
        });

        var bootstrapper = CreateBootstrapper(scope);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => bootstrapper.InitializeAsync(CancellationToken.None));

        exception.Message.ShouldContain("bootstrap application");
        exception.Message.ShouldContain("secret");
    }

    [Fact]
    public async Task InitializeAsync_WhenBootstrapAdministratorEmailConflicts_FailsClearly()
    {
        using var provider = TestServiceProviderFactory.Create(
            Guid.NewGuid().ToString(),
            configureOptions: options =>
            {
                options.ManagementBootstrap.Enabled = true;
                options.ManagementBootstrap.ClientId = "bootstrap-client";
                options.ManagementBootstrap.ClientSecret = "bootstrap-secret";
                options.ManagementBootstrap.AdminUserName = "administrator";
                options.ManagementBootstrap.AdminEmail = "configured@example.com";
                options.ManagementBootstrap.AdminPassword = "Password1!";
            });

        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<SecurityService.Database.Entities.ApplicationUser>>();
        (await userManager.CreateAsync(new SecurityService.Database.Entities.ApplicationUser
        {
            UserName = "administrator",
            Email = "existing@example.com"
        }, "Password1!"))
            .Succeeded.ShouldBeTrue();

        var bootstrapper = CreateBootstrapper(scope);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => bootstrapper.InitializeAsync(CancellationToken.None));

        exception.Message.ShouldContain("bootstrap administrator");
        exception.Message.ShouldContain("email");
    }

    private static ManagementBootstrapper CreateBootstrapper(IServiceScope scope)
    {
        return new ManagementBootstrapper(
            scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>(),
            scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>(),
            scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<SecurityService.Database.Entities.ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>(),
            scope.ServiceProvider.GetRequiredService<IOptions<ServiceOptions>>());
    }
}
