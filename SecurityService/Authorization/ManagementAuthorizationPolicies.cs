using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;

namespace SecurityService.Authorization;

public static class ManagementAuthorizationPolicies
{
    public static string ManagementApi { get; } = "ManagementApi";

    public static void AddManagementApiPolicy(AuthorizationOptions options)
    {
        options.AddPolicy(ManagementApi, policy =>
            policy.AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
                  .RequireAuthenticatedUser());
    }
}
