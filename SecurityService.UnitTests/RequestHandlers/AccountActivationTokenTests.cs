using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database.Entities;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;

namespace SecurityService.UnitTests.RequestHandlers;

public sealed class AccountActivationTokenTests
{
    [Fact]
    public async Task ActivationToken_CannotBeReusedAfterPasswordIsSet()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.ActivationToken_CannotBeReusedAfterPasswordIsSet));
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "alice", Email = "alice@example.com", EmailConfirmed = true };

        (await userManager.CreateAsync(user)).Succeeded.ShouldBeTrue();
        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        (await userManager.ResetPasswordAsync(user, token, "NewPassword1!")).Succeeded.ShouldBeTrue();
        (await userManager.ResetPasswordAsync(user, token, "AnotherPassword1!")).Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task ActivationToken_ExpiresAccordingToConfiguredLifetime()
    {
        using var provider = TestServiceProviderFactory.Create(
            nameof(this.ActivationToken_ExpiresAccordingToConfiguredLifetime),
            passwordTokenLifespan: TimeSpan.FromMilliseconds(1));
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "alice", Email = "alice@example.com", EmailConfirmed = true };

        (await userManager.CreateAsync(user)).Succeeded.ShouldBeTrue();
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        await Task.Delay(25);

        (await userManager.ResetPasswordAsync(user, token, "NewPassword1!")).Succeeded.ShouldBeFalse();
    }
}
