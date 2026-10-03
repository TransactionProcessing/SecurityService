using Microsoft.AspNetCore.Authorization;

namespace SecurityService.Authorization;

public static class ManagementAuthorizationPolicies
{
    public const string ManagementApi = "ManagementApi";

    public static void AddManagementApiPolicy(AuthorizationOptions options)
    {
        options.AddPolicy(ManagementApi, policy => policy.RequireAuthenticatedUser());
    }
}
