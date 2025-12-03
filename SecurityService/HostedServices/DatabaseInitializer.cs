using System;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SecurityService.Database.DbContexts;
using Microsoft.Extensions.Logging;
using Shared.Logger;

public class DatabaseInitializer : IHostedService, IDisposable
{
    private readonly IServiceProvider Services;
    private readonly IHostApplicationLifetime Lifetime;
    private readonly CancellationTokenSource InternalCts = new();
    private Task? StartupTask;

    public DatabaseInitializer(IServiceProvider services, IHostApplicationLifetime lifetime) {
        Services = services;
        this.Lifetime = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // start migrations/seeding on the threadpool so StartAsync returns quickly if you want.
        StartupTask = Task.Run(() => RunMigrationsAsync(InternalCts.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    private async Task RunMigrationsAsync(CancellationToken token)
    {
        try {

            Logger.LogWarning("About to start DatabaseMigrations");

            using IServiceScope scope = Services.CreateScope();
            PersistedGrantDbContext persistedGrant = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
            ConfigurationDbContext config = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            AuthenticationDbContext auth = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();

            if (persistedGrant.Database.IsRelational()) {
                await persistedGrant.Database.MigrateAsync(token);
                await persistedGrant.SetDbInSimpleMode(token);
            }

            if (config.Database.IsRelational()){
                await config.Database.MigrateAsync(token);
                await config.SetDbInSimpleMode(token);
            }

            if (auth.Database.IsRelational()) {
                await auth.Database.MigrateAsync(token);
                await auth.SetDbInSimpleMode(token);
            }

            Logger.LogWarning("Database Migrations Successful");
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            Logger.LogInformation("DatabaseInitializer migration canceled.");
        }
        catch (Exception ex)
        {
            Logger.LogError("DatabaseInitializer migration failed.", ex);
            // Request host shutdown:
            try
            {
                Environment.ExitCode = 1;
                Lifetime.StopApplication();
            }
            catch
            {
                // ignore any exception from StopApplication, we are already failing
            }

            // Rethrow to ensure host doesn't continue starting successfully.
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // per-service shutdown window
        TimeSpan perServiceTimeout = TimeSpan.FromSeconds(20);

        // request cancel of internal work
        await this.InternalCts.CancelAsync();

        if (StartupTask == null)
            return;

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task timeout = Task.Delay(perServiceTimeout, linked.Token);
        Task completed = await Task.WhenAny(StartupTask, timeout);

        if (completed == timeout)
        {
            Logger.LogWarning($"DatabaseInitializer did not finish within {perServiceTimeout.TotalSeconds}s; continuing shutdown.");
            // host will continue shutdown; hosted service StopAsync returned after per-service timeout.
        }
        else
        {
            await linked.CancelAsync(); // cancel the delay
            await StartupTask; // propagate exceptions if any
        }
    }

    public void Dispose() => InternalCts.Dispose();
}