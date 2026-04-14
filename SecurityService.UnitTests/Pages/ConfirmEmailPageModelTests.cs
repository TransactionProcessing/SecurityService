using MessagingService.Client;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SecurityService.BusinessLogic;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;
using SecurityService.Database.Entities;

namespace SecurityService.UnitTests.Pages;

public class ConfirmEmailPageModelTests
{
    [Fact]
    public async Task OnGet_WithQueryValues_PopulatesInput()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.ConfirmEmailAsync(user, "token-123"))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(instance => instance.RemovePasswordAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(instance => instance.AddPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = CreateProvider(nameof(this.OnGet_WithQueryValues_PopulatesInput), userManager, signInManager);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?userName=alice&confirmationToken=token-123");

        var model = CreateModel(provider, httpContext);

        var result = await model.OnGet(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.Input.Username.ShouldBe("alice");
        model.Input.Token.ShouldBe("token-123");
    }

    [Fact]
    public async Task OnGet_WithMissingQueryValues_ReturnsPageAndDoesNotCallMediator()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = CreateProvider(nameof(this.OnGet_WithMissingQueryValues_ReturnsPageAndDoesNotCallMediator), userManager, signInManager);
        var model = CreateModel(provider, new DefaultHttpContext());

        var result = await model.OnGet(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.View.UserMessage.ShouldBe("The email confirmation link is invalid.");
        userManager.Verify(instance => instance.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnGet_WhenConfirmationFails_ReturnsPageWithFailureMessage()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.ConfirmEmailAsync(user, "token-123"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "failed" }));

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = CreateProvider(nameof(this.OnGet_WhenConfirmationFails_ReturnsPageWithFailureMessage), userManager, signInManager);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?userName=alice&confirmationToken=token-123");
        var model = CreateModel(provider, httpContext);

        var result = await model.OnGet(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.View.UserMessage.ShouldBe("Failed confirming user email address for username alice");
        model.Input.Username.ShouldBe("alice");
        model.Input.Token.ShouldBe("token-123");
        userManager.Verify(instance => instance.ConfirmEmailAsync(user, "token-123"), Times.Once);
        userManager.Verify(instance => instance.RemovePasswordAsync(It.IsAny<ApplicationUser>()), Times.Never);
        userManager.Verify(instance => instance.AddPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnGet_WhenConfirmationSucceeds_ReturnsPageAndSendsExpectedCommands()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.ConfirmEmailAsync(user, "token-123"))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(instance => instance.RemovePasswordAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(instance => instance.AddPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = CreateProvider(nameof(this.OnGet_WhenConfirmationSucceeds_ReturnsPageAndSendsExpectedCommands), userManager, signInManager);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?userName=alice&confirmationToken=token-123");
        var model = CreateModel(provider, httpContext);

        var result = await model.OnGet(CancellationToken.None);
        var messagingClient = provider.GetRequiredService<IMessagingServiceClient>().ShouldBeOfType<TestMessagingServiceClient>();

        result.ShouldBeOfType<PageResult>();
        model.Input.Username.ShouldBe("alice");
        model.Input.Token.ShouldBe("token-123");
        model.View.UserMessage.ShouldBe("Thanks for confirming your email address, you should receive a welcome email soon.");
        userManager.Verify(instance => instance.ConfirmEmailAsync(user, "token-123"), Times.Once);
        userManager.Verify(instance => instance.RemovePasswordAsync(user), Times.Once);
        userManager.Verify(instance => instance.AddPasswordAsync(user, It.IsAny<string>()), Times.Once);
        messagingClient.LastEmailRequest.ShouldNotBeNull();
        messagingClient.LastEmailRequest.Subject.ShouldBe("Welcome to Transaction Processing");
        messagingClient.LastEmailRequest.ToAddresses.ShouldContain("alice@example.com");
    }

    private static ServiceProvider CreateProvider(string databaseName,
                                                  Mock<UserManager<ApplicationUser>> userManager,
                                                  Mock<SignInManager<ApplicationUser>> signInManager)
    {
        return TestServiceProviderFactory.Create(databaseName, userManager.Object, signInManager.Object);
    }

    private static SecurityService.Pages.Account.ConfirmEmail.IndexModel CreateModel(ServiceProvider provider, HttpContext httpContext)
    {
        return new SecurityService.Pages.Account.ConfirmEmail.IndexModel(provider.GetRequiredService<IMediator>())
        {
            PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
            {
                HttpContext = httpContext
            }
        };
    }
}
