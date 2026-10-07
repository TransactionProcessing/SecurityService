using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SecurityService.BusinessLogic.Mfa;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;

namespace SecurityService.UnitTests.Authorization;

public sealed class MfaPolicyServiceTests
{
    [Fact]
    public async Task IsMfaRequired_returns_false_when_user_and_roles_have_no_policy()
    {
        using var provider = TestServiceProviderFactory.Create(Guid.NewGuid().ToString());
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "mfa-user" };
        (await userManager.CreateAsync(user)).Succeeded.ShouldBeTrue();

        var service = CreateService(scope);

        (await service.IsMfaRequiredAsync(user, CancellationToken.None)).ShouldBeFalse();
    }

    [Fact]
    public async Task IsMfaRequired_returns_true_for_direct_user_policy()
    {
        using var provider = TestServiceProviderFactory.Create(Guid.NewGuid().ToString());
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "mfa-user" };
        (await userManager.CreateAsync(user)).Succeeded.ShouldBeTrue();
        var service = CreateService(scope);

        (await service.RequireUserAsync(user.Id, CancellationToken.None)).IsSuccess.ShouldBeTrue();

        (await service.IsMfaRequiredAsync(user, CancellationToken.None)).ShouldBeTrue();
    }

    [Fact]
    public async Task IsMfaRequired_returns_true_for_any_role_policy()
    {
        using var provider = TestServiceProviderFactory.Create(Guid.NewGuid().ToString());
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var user = new ApplicationUser { UserName = "mfa-user" };
        (await userManager.CreateAsync(user)).Succeeded.ShouldBeTrue();
        (await roleManager.CreateAsync(new IdentityRole("Estate"))).Succeeded.ShouldBeTrue();
        (await userManager.AddToRoleAsync(user, "Estate")).Succeeded.ShouldBeTrue();
        var service = CreateService(scope);

        var role = await roleManager.FindByNameAsync("Estate");
        (await service.RequireRoleAsync(role!.Id, CancellationToken.None)).IsSuccess.ShouldBeTrue();

        (await service.IsMfaRequiredAsync(user, CancellationToken.None)).ShouldBeTrue();
    }

    [Fact]
    public async Task Require_and_remove_operations_are_idempotent()
    {
        using var provider = TestServiceProviderFactory.Create(Guid.NewGuid().ToString());
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "mfa-user" };
        (await userManager.CreateAsync(user)).Succeeded.ShouldBeTrue();
        var service = CreateService(scope);

        (await service.RequireUserAsync(user.Id, CancellationToken.None)).IsSuccess.ShouldBeTrue();
        (await service.RequireUserAsync(user.Id, CancellationToken.None)).IsSuccess.ShouldBeTrue();
        (await service.RemoveUserAsync(user.Id, CancellationToken.None)).IsSuccess.ShouldBeTrue();
        (await service.RemoveUserAsync(user.Id, CancellationToken.None)).IsSuccess.ShouldBeTrue();

        (await service.IsMfaRequiredAsync(user, CancellationToken.None)).ShouldBeFalse();
    }

    private static MfaPolicyService CreateService(IServiceScope scope)
    {
        return new MfaPolicyService(
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
            scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>());
    }
}
