using MessagingService.Client;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Imposter.Abstractions;
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
        userManager.FindByNameAsync("alice")
            .ReturnsAsync(user);
        userManager.ConfirmEmailAsync(user, "token-123")
            .ReturnsAsync(IdentityResult.Success);
        userManager.RemovePasswordAsync(user)
            .ReturnsAsync(IdentityResult.Success);
        userManager.AddPasswordAsync(user, Arg<string>.Any())
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
        userManager.FindByNameAsync(Arg<string>.Any()).Called(Count.Never());
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
        userManager.FindByNameAsync("alice")
            .ReturnsAsync(user);
        userManager.ConfirmEmailAsync(user, "token-123")
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
        userManager.ConfirmEmailAsync(user, "token-123").Called(Count.Once());
        userManager.RemovePasswordAsync(Arg<ApplicationUser>.Any()).Called(Count.Never());
        userManager.AddPasswordAsync(Arg<ApplicationUser>.Any(), Arg<string>.Any()).Called(Count.Never());
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
        userManager.FindByNameAsync("alice")
            .ReturnsAsync(user);
        userManager.ConfirmEmailAsync(user, "token-123")
            .ReturnsAsync(IdentityResult.Success);
        userManager.RemovePasswordAsync(user)
            .ReturnsAsync(IdentityResult.Success);
        userManager.AddPasswordAsync(user, Arg<string>.Any())
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
        userManager.ConfirmEmailAsync(user, "token-123").Called(Count.Once());
        userManager.RemovePasswordAsync(user).Called(Count.Once());
        userManager.AddPasswordAsync(user, Arg<string>.Any()).Called(Count.Once());
        messagingClient.LastEmailRequest.ShouldNotBeNull();
        messagingClient.LastEmailRequest.Subject.ShouldBe("Welcome to Transaction Processing");
        messagingClient.LastEmailRequest.ToAddresses.ShouldContain("alice@example.com");
    }

    private static ServiceProvider CreateProvider(string databaseName,
                                                  UserManagerDouble userManager,
                                                  SignInManagerDouble signInManager)
    {
        return TestServiceProviderFactory.Create(databaseName, userManager.Instance(), signInManager.Instance());
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
