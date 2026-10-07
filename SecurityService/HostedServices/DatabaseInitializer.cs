using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.Database;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using Shared.Logger;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.HostedServices;

public sealed class DatabaseInitializer : IHostedService
{
    private readonly IServiceProvider ServiceProvider;
    private readonly ServiceOptions Options;

    public DatabaseInitializer(IServiceProvider serviceProvider, IOptions<ServiceOptions> options)
    {
        this.ServiceProvider = serviceProvider;
        this.Options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try {
            Logger.LogWarning("Starting database initialization for security service.");
            using IServiceScope scope = this.ServiceProvider.CreateScope();

            SecurityServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();
            if (this.Options.UseInMemoryDatabase) {
                Logger.LogInformation($"Using in-memory database '{this.Options.InMemoryDatabaseName}'.");
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            }
            else {
                Logger.LogInformation("Applying database migrations.");
                await dbContext.Database.MigrateAsync(cancellationToken);
            }

            if (this.Options.SeedDefaultScopes == false) {
                Logger.LogInformation("Default scope seeding is disabled.");
            }
            else {
                IOpenIddictScopeManager scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
                (string Name, string DisplayName, string Description)[] defaultScopes = [(Scopes.Profile, "Profile", "Access to the user's profile information."), (Scopes.Email, "Email", "Access to the user's email address."), (Scopes.Roles, "Roles", "Access to the user's role membership."), (Scopes.OpenId, "OpenId", "Required OpenID Connect subject access."), (Scopes.OfflineAccess, "Offline access", "Access to refresh tokens.")];

                foreach ((string name, string displayName, string description) in defaultScopes) {
                    if (await scopeManager.FindByNameAsync(name, cancellationToken) is null) {
                        Logger.LogInformation($"Creating default scope {name}.");
                        await scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = name, DisplayName = displayName, Description = description }, cancellationToken);
                    }
                }
            }

            await this.SynchronizeApplicationPermissionsAsync(scope.ServiceProvider, dbContext, cancellationToken);

            ManagementBootstrapper bootstrapper = scope.ServiceProvider.GetRequiredService<ManagementBootstrapper>();
            await bootstrapper.InitializeAsync(cancellationToken);

            Logger.LogWarning("Database initialization complete.");
        }
        catch (Exception ex) {
            Logger.LogError(new Exception("An error occurred during database initialization.", ex));
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SynchronizeApplicationPermissionsAsync(IServiceProvider serviceProvider, SecurityServiceDbContext dbContext, CancellationToken cancellationToken)
    {
        IOpenIddictApplicationManager applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        List<ClientDefinition> clients = await dbContext.ClientDefinitions.AsNoTracking().ToListAsync(cancellationToken);

        foreach (ClientDefinition client in clients)
        {
            object? application = await applicationManager.FindByClientIdAsync(client.ClientId, cancellationToken);
            if (application is null)
            {
                continue;
            }

            OpenIddictApplicationDescriptor descriptor = new();
            await applicationManager.PopulateAsync(descriptor, application, cancellationToken);

            IReadOnlyCollection<string> allowedGrantTypes = JsonListSerializer.Deserialize(client.AllowedGrantTypesJson);
            if (this.Options.OAuth.EnableLegacyGrantTypes == false || this.Options.OAuth.EnableHybridFlow == false)
            {
                allowedGrantTypes = allowedGrantTypes
                    .Where(grantType =>
                        (this.Options.OAuth.EnableLegacyGrantTypes ||
                         (string.Equals(grantType, "password", StringComparison.OrdinalIgnoreCase) == false &&
                          string.Equals(grantType, "implicit", StringComparison.OrdinalIgnoreCase) == false &&
                          string.Equals(grantType, "hybrid", StringComparison.OrdinalIgnoreCase) == false)) &&
                        (this.Options.OAuth.EnableHybridFlow ||
                         string.Equals(grantType, "hybrid", StringComparison.OrdinalIgnoreCase) == false))
                    .ToArray();
            }

            OAuthGrantPolicyResult policy = OAuthGrantPolicy.CreatePermissions(
                allowedGrantTypes,
                client.ClientId,
                this.Options.OAuth);

            descriptor.Permissions.RemoveWhere(permission => permission.StartsWith(Permissions.Prefixes.GrantType, StringComparison.Ordinal));
            descriptor.Permissions.UnionWith(policy.Permissions);

            if (client.AllowOfflineAccess)
            {
                descriptor.Permissions.Add(Permissions.GrantTypes.RefreshToken);
            }

            descriptor.Requirements.UnionWith(policy.Requirements);

            await applicationManager.UpdateAsync(application, descriptor, cancellationToken);
        }
    }
}
