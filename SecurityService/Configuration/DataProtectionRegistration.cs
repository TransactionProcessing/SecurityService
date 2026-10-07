using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;

namespace SecurityService.Configuration;

public static class DataProtectionRegistration
{
    public static void Add(IServiceCollection services, IConfiguration configuration)
    {
        var dataProtectionKeyDirectory = configuration["ServiceOptions:DataProtectionKeyDirectory"];
        var dataProtectionBuilder = services.AddDataProtection();
        if (string.IsNullOrWhiteSpace(dataProtectionKeyDirectory) == false)
        {
            dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyDirectory));
        }
    }
}
