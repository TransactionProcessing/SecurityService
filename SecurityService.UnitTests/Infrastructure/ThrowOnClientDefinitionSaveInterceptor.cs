using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SecurityService.Database.Entities;

namespace SecurityService.UnitTests.Infrastructure;

internal sealed class ThrowOnClientDefinitionSaveInterceptor : SaveChangesInterceptor
{
    private readonly Exception _exception;
    private readonly bool _throwOnOpenIddictApplicationSave;

    public ThrowOnClientDefinitionSaveInterceptor(Exception? exception = null, bool throwOnOpenIddictApplicationSave = false)
    {
        this._exception = exception ?? new InvalidOperationException("Injected ClientDefinition persistence failure.");
        this._throwOnOpenIddictApplicationSave = throwOnOpenIddictApplicationSave;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ThrowIfClientDefinitionIsBeingAdded(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ThrowIfClientDefinitionIsBeingAdded(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ThrowIfClientDefinitionIsBeingAdded(DbContext? context)
    {
        bool clientDefinitionAdded =
            context?.ChangeTracker.Entries<ClientDefinition>().Any(entry => entry.State == EntityState.Added) == true;
        bool openIddictApplicationAdded = this._throwOnOpenIddictApplicationSave &&
            context?.ChangeTracker.Entries().Any(entry => entry.State == EntityState.Added &&
                entry.Entity.GetType().Name.Contains("Application", StringComparison.Ordinal)) == true;

        if (clientDefinitionAdded || openIddictApplicationAdded)
        {
            throw this._exception;
        }
    }
}
