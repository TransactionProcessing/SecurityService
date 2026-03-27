using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic;
using SecurityService.Database.DbContexts;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.HostedServices;

public sealed class DatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceOptions _options;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IServiceProvider serviceProvider, IOptions<ServiceOptions> options, ILogger<DatabaseInitializer> logger)
    {
        this._serviceProvider = serviceProvider;
        this._options = options.Value;
        this._logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this._logger.LogInformation("Starting database initialization for security service.");
        using IServiceScope scope = this._serviceProvider.CreateScope();

        SecurityServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();
        if (this._options.UseInMemoryDatabase)
        {
            this._logger.LogInformation("Using in-memory database '{DatabaseName}'.", this._options.InMemoryDatabaseName);
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
        else
        {
            this._logger.LogInformation("Applying database migrations.");
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        if (this._options.SeedDefaultScopes == false)
        {
            this._logger.LogInformation("Default scope seeding is disabled.");
            return;
        }

        IOpenIddictScopeManager scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
        (string Name, string DisplayName, string Description)[] defaultScopes =
        {
            (Scopes.Profile, "Profile", "Access to the user's profile information."),
            (Scopes.Email, "Email", "Access to the user's email address."),
            (Scopes.Roles, "Roles", "Access to the user's role membership."),
            (Scopes.OpenId, "OpenId", "Required OpenID Connect subject access."),
            (Scopes.OfflineAccess, "Offline access", "Access to refresh tokens.")
        };

        foreach ((string name, string displayName, string description) in defaultScopes)
        {
            if (await scopeManager.FindByNameAsync(name, cancellationToken) is null)
            {
                this._logger.LogInformation("Creating default scope {ScopeName}.", name);
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = name,
                    DisplayName = displayName,
                    Description = description
                }, cancellationToken);
            }
        }

        this._logger.LogInformation("Database initialization complete.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
