using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic;
using SecurityService.Database.DbContexts;
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

            Logger.LogWarning("Database initialization complete.");
        }
        catch (Exception ex) {
            Logger.LogError(new Exception("An error occurred during database initialization.", ex));
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
