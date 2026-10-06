using Microsoft.AspNetCore.DataProtection;
using SecurityService.BusinessLogic.Mfa;
using Shouldly;

namespace SecurityService.UnitTests.Mfa;

public sealed class MfaSignInTransactionProtectorTests
{
    [Fact]
    public void Protect_RoundTripsTransactionWithoutExposingItsPayload()
    {
        var protector = new MfaSignInTransactionProtector(new EphemeralDataProtectionProvider());

        var token = protector.Protect("user-1", "/connect/authorize?client_id=client", rememberLogin: true);

        protector.TryUnprotect(token, out var transaction).ShouldBeTrue();
        transaction.ShouldNotBeNull();
        transaction.UserId.ShouldBe("user-1");
        transaction.ReturnUrl.ShouldBe("/connect/authorize?client_id=client");
        transaction.RememberLogin.ShouldBeTrue();
        token.ShouldNotContain("user-1");
    }

    [Fact]
    public void TryUnprotect_RejectsTamperedTransaction()
    {
        var protector = new MfaSignInTransactionProtector(new EphemeralDataProtectionProvider());
        var token = protector.Protect("user-1", "/", rememberLogin: false);

        protector.TryUnprotect(token + "tampered", out _).ShouldBeFalse();
    }
}
