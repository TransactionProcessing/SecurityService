using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecurityService.Configuration;
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

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ServiceOptions:DataProtectionKeyDirectory"] = this._temporaryDirectory
            })
            .Build();

        var firstServices = new ServiceCollection();
        DataProtectionRegistration.Add(firstServices, configuration);
        var firstProvider = firstServices.BuildServiceProvider().GetRequiredService<IDataProtectionProvider>();
        var firstProtector = firstProvider.CreateProtector("SecurityService.Tests");
        var protectedValue = firstProtector.Protect("sensitive-value");

        var secondServices = new ServiceCollection();
        DataProtectionRegistration.Add(secondServices, configuration);
        var secondProvider = secondServices.BuildServiceProvider().GetRequiredService<IDataProtectionProvider>();
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
