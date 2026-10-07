using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SecurityService.Database.Entities;

namespace SecurityService.UnitTests.Infrastructure;

internal sealed class ThrowOnClientDefinitionSaveInterceptor : SaveChangesInterceptor
{
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

    private static void ThrowIfClientDefinitionIsBeingAdded(DbContext? context)
    {
        if (context?.ChangeTracker.Entries<ClientDefinition>().Any(entry => entry.State == EntityState.Added) == true)
        {
            throw new InvalidOperationException("Injected ClientDefinition persistence failure.");
        }
    }
}
