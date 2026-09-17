using Microsoft.AspNetCore.DataProtection;
using SecurityService.BusinessLogic.Oidc;
using Shouldly;

namespace SecurityService.UnitTests.Oidc;

public sealed class ConsentTransactionProtectorTests
{
    [Fact]
    public void ProtectAndUnprotect_RoundTripsTransaction()
    {
        var protector = new ConsentTransactionProtector(
            new EphemeralDataProtectionProvider().CreateProtector("consent"));
        var transaction = new ConsentTransaction(
            UserId: "user-1",
            AuthorizationUrl: "/connect/authorize?client_id=client&scope=openid%20profile",
            ClientId: "client",
            RequestedScopes: ["openid", "profile"],
            ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(5));

        var token = protector.Protect(transaction);
        var restored = protector.Unprotect(token, DateTimeOffset.UtcNow);

        restored.ShouldNotBeNull();
        restored.UserId.ShouldBe("user-1");
        restored.AuthorizationUrl.ShouldBe(transaction.AuthorizationUrl);
        restored.ClientId.ShouldBe("client");
        restored.RequestedScopes.ShouldBe(["openid", "profile"]);
    }

    [Fact]
    public void Unprotect_WhenTokenIsInvalid_ReturnsNull()
    {
        var protector = new ConsentTransactionProtector(
            new EphemeralDataProtectionProvider().CreateProtector("consent"));

        protector.Unprotect("not-a-consent-token", DateTimeOffset.UtcNow).ShouldBeNull();
    }

    [Fact]
    public void Unprotect_WhenTransactionIsExpired_ReturnsNull()
    {
        var protector = new ConsentTransactionProtector(
            new EphemeralDataProtectionProvider().CreateProtector("consent"));
        var token = protector.Protect(new ConsentTransaction(
            "user-1", "/connect/authorize", "client", ["openid"], DateTimeOffset.UtcNow.AddMinutes(-1)));

        protector.Unprotect(token, DateTimeOffset.UtcNow).ShouldBeNull();
    }
}
