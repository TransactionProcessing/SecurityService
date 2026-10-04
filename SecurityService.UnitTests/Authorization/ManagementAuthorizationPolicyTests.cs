using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using OpenIddict.Validation.AspNetCore;
using SecurityService.Authorization;
using Shouldly;

namespace SecurityService.UnitTests.Authorization;

public sealed class ManagementAuthorizationPolicyTests
{
    [Fact]
    public void AddManagementApiPolicy_RequiresAnAuthenticatedUser()
    {
        var options = new AuthorizationOptions();

        ManagementAuthorizationPolicies.AddManagementApiPolicy(options);

        var policy = options.GetPolicy(ManagementAuthorizationPolicies.ManagementApi);

        policy.ShouldNotBeNull();
        policy!.Requirements.ShouldContain(requirement => requirement is DenyAnonymousAuthorizationRequirement);
        policy.AuthenticationSchemes.ShouldContain(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
    }
}
