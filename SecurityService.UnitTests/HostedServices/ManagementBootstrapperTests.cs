using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic;
using SecurityService.Database;
using SecurityService.Database.Entities;
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
    public async Task DatabaseInitialization_WhenBootstrapDefinitionHasStaleGrants_LeavesClientCredentialsOnly()
    {
        const string databaseName = "bootstrap-permissions-";
        using var provider = TestServiceProviderFactory.Create(
            databaseName + Guid.NewGuid(),
            configureOptions: options =>
            {
                options.UseInMemoryDatabase = true;
                options.ManagementBootstrap.Enabled = true;
                options.ManagementBootstrap.ClientId = "bootstrap-client";
                options.ManagementBootstrap.ClientSecret = "bootstrap-secret";
            });

        using var scope = provider.CreateScope();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "bootstrap-client",
            ClientSecret = "bootstrap-secret",
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.ResponseTypes.Code
            }
        });

        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();
        await dbContext.ClientDefinitions.AddAsync(new ClientDefinition
        {
            Id = Guid.NewGuid(),
            ClientId = "bootstrap-client",
            ClientName = "Stale Bootstrap Client",
            SecretHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("bootstrap-secret"))),
            AllowedGrantTypesJson = JsonListSerializer.Serialize([OpenIddictConstants.GrantTypes.AuthorizationCode]),
            AllowedScopesJson = "[]",
            RedirectUrisJson = "[]",
            PostLogoutRedirectUrisJson = "[]",
            ClientType = OpenIddictConstants.ClientTypes.Confidential
        });
        await dbContext.SaveChangesAsync();

        var initializer = new DatabaseInitializer(
            provider,
            scope.ServiceProvider.GetRequiredService<IOptions<ServiceOptions>>());

        await initializer.StartAsync(CancellationToken.None);

        object application = (await applicationManager.FindByClientIdAsync("bootstrap-client"))!;
        var permissions = (await applicationManager.GetPermissionsAsync(application)).ToHashSet(StringComparer.Ordinal);
        permissions.ShouldContain(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
        permissions.ShouldNotContain(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
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
