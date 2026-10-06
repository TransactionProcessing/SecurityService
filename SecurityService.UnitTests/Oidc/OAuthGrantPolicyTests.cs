using OpenIddict.Abstractions;
using SecurityService.BusinessLogic;
using SecurityService.BusinessLogic.Oidc;
using Shouldly;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.UnitTests.Oidc;

public sealed class OAuthGrantPolicyTests
{
    [Fact]
    public void AuthorizationCodeAndRefreshToken_CreateApprovedPkcePermissions()
    {
        OAuthGrantPolicyResult result = OAuthGrantPolicy.CreatePermissions(
            [GrantTypes.AuthorizationCode, GrantTypes.RefreshToken],
            "browser-client",
            new OAuthOptions());

        result.Permissions.ShouldContain(Permissions.Endpoints.Authorization);
        result.Permissions.ShouldContain(Permissions.Endpoints.Token);
        result.Permissions.ShouldContain(Permissions.GrantTypes.AuthorizationCode);
        result.Permissions.ShouldContain(Permissions.GrantTypes.RefreshToken);
        result.Permissions.ShouldContain(Permissions.ResponseTypes.Code);
        result.Requirements.ShouldContain(Requirements.Features.ProofKeyForCodeExchange);
    }

    [Fact]
    public void ClientCredentials_CreateTokenPermissionOnly()
    {
        OAuthGrantPolicyResult result = OAuthGrantPolicy.CreatePermissions(
            [GrantTypes.ClientCredentials],
            "service-client",
            new OAuthOptions());

        result.Permissions.ShouldContain(Permissions.Endpoints.Token);
        result.Permissions.ShouldContain(Permissions.GrantTypes.ClientCredentials);
        result.Permissions.ShouldNotContain(Permissions.Endpoints.Authorization);
        result.Requirements.ShouldBeEmpty();
    }

    [Fact]
    public void LegacyGrants_AreRejectedUnlessClientIsConfigured()
    {
        var options = new OAuthOptions();

        OAuthGrantPolicy.IsGrantAllowed(GrantTypes.Password, "mobile-client", options).ShouldBeFalse();
        OAuthGrantPolicy.IsGrantAllowed("hybrid", "browser-client", options).ShouldBeFalse();
        OAuthGrantPolicy.IsGrantAllowed(GrantTypes.Implicit, "browser-client", options).ShouldBeFalse();
        OAuthGrantPolicy.IsGrantAllowed(GrantTypes.DeviceCode, "device-client", options).ShouldBeFalse();

        options.LegacyGrantTypeClients[GrantTypes.Password] = ["mobile-client"];

        OAuthGrantPolicy.IsGrantAllowed(GrantTypes.Password, "mobile-client", options).ShouldBeTrue();
        OAuthGrantPolicy.IsGrantAllowed(GrantTypes.Password, "other-client", options).ShouldBeFalse();
    }

    [Fact]
    public void UnknownGrant_IsRejected()
    {
        OAuthGrantPolicy.IsGrantAllowed("custom-grant", "client", new OAuthOptions()).ShouldBeFalse();
    }
}
