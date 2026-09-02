using Imposter.Abstractions;
using SecurityService.Database.Entities;
using Shouldly;

namespace SecurityService.UnitTests.Infrastructure;

public class IdentityMocksTests
{
    [Fact]
    public async Task UserManagerDouble_UsesConfiguredArgumentMatchers()
    {
        var user = new ApplicationUser { UserName = "alice" };
        var userManager = IdentityMocks.CreateUserManager();

        userManager.FindByNameAsync("alice").ReturnsAsync(user);

        (await userManager.Instance().FindByNameAsync("bob")).ShouldBeNull();
        (await userManager.Instance().FindByNameAsync("alice")).ShouldBeSameAs(user);
    }

    [Fact]
    public void UserManagerDouble_CalledFailsWhenCountDoesNotMatch()
    {
        var userManager = IdentityMocks.CreateUserManager();

        Should.Throw<InvalidOperationException>(() =>
            userManager.FindByNameAsync("alice").Called(Count.Once()));
    }
}
