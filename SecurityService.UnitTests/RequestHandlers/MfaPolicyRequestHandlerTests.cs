using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SecurityService.BusinessLogic.Mfa;
using SecurityService.BusinessLogic.RequestHandlers;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database.Entities;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;

namespace SecurityService.UnitTests.RequestHandlers;

public sealed class MfaPolicyRequestHandlerTests
{
    [Fact]
    public async Task User_policy_commands_are_idempotent_and_remove_the_policy()
    {
        using var provider = TestServiceProviderFactory.Create(Guid.NewGuid().ToString());
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "mfa-user" };
        (await userManager.CreateAsync(user)).Succeeded.ShouldBeTrue();

        var handler = new MfaPolicyRequestHandler(CreateService(scope));

        (await handler.Handle(new SecurityServiceCommands.RequireUserMfaCommand(user.Id), CancellationToken.None)).IsSuccess.ShouldBeTrue();
        (await handler.Handle(new SecurityServiceCommands.RequireUserMfaCommand(user.Id), CancellationToken.None)).IsSuccess.ShouldBeTrue();
        (await handler.Handle(new SecurityServiceCommands.RemoveUserMfaCommand(user.Id), CancellationToken.None)).IsSuccess.ShouldBeTrue();

        var service = CreateService(scope);
        (await service.IsMfaRequiredAsync(user, CancellationToken.None)).ShouldBeFalse();
    }

    private static MfaPolicyService CreateService(IServiceScope scope)
    {
        return new MfaPolicyService(
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
            scope.ServiceProvider.GetRequiredService<SecurityService.Database.DbContexts.SecurityServiceDbContext>());
    }
}
