using Microsoft.AspNetCore.DataProtection;
using Shouldly;

namespace SecurityService.UnitTests.Configuration;

public sealed class DataProtectionPersistenceTests : IDisposable
{
    private readonly string _temporaryDirectory = Path.Combine(
        Path.GetTempPath(),
        $"securityservice-keys-{Guid.NewGuid():N}");

    [Fact]
    public void PersistedKeys_CanUnprotectDataCreatedByAnotherProviderInstance()
    {
        Directory.CreateDirectory(this._temporaryDirectory);

        var firstProvider = DataProtectionProvider.Create(new DirectoryInfo(this._temporaryDirectory));
        var firstProtector = firstProvider.CreateProtector("SecurityService.Tests");
        var protectedValue = firstProtector.Protect("sensitive-value");

        var secondProvider = DataProtectionProvider.Create(new DirectoryInfo(this._temporaryDirectory));
        var secondProtector = secondProvider.CreateProtector("SecurityService.Tests");

        secondProtector.Unprotect(protectedValue).ShouldBe("sensitive-value");
    }

    public void Dispose()
    {
        if (Directory.Exists(this._temporaryDirectory))
        {
            Directory.Delete(this._temporaryDirectory, recursive: true);
        }
    }
}
