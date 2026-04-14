using MessagingService.Client;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SecurityService.BusinessLogic;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;
using SecurityService.Database.Entities;

namespace SecurityService.UnitTests.Pages;

public class ForgotPasswordPageModelTests
{
    [Fact]
    public async Task OnGet_WithReturnUrlAndClientIdQuery_PopulatesInput()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = CreateProvider(nameof(this.OnGet_WithReturnUrlAndClientIdQuery_PopulatesInput), userManager, signInManager);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?clientId=test-client-id");
        var model = CreateModel(provider, httpContext);

        var result = await model.OnGet("/connect/authorize?client_id=test-client-id");

        result.ShouldBeOfType<PageResult>();
        model.Input.ReturnUrl.ShouldBe("/connect/authorize?client_id=test-client-id");
        model.Input.ClientId.ShouldBe("test-client-id");
    }

    [Fact]
    public async Task OnPost_WhenCancelButtonSelected_RedirectsToLoginAndDoesNotCallMediatorPath()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = CreateProvider(nameof(this.OnPost_WhenCancelButtonSelected_RedirectsToLoginAndDoesNotCallMediatorPath), userManager, signInManager);
        var model = CreateModel(provider, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Account.ForgotPassword.IndexInputModel
        {
            Button = "cancel",
            EmailAddress = "alice",
            ClientId = "test-client-id"
        };

        var result = await model.OnPost(CancellationToken.None);

        var redirect = result.ShouldBeOfType<RedirectResult>();
        redirect.Url.ShouldBe("Login/Index");
        userManager.Verify(instance => instance.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_WhenMediatorPathSucceeds_ReturnsPageAndSendsResetEmail()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("token+/=");

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = CreateProvider(nameof(this.OnPost_WhenMediatorPathSucceeds_ReturnsPageAndSendsResetEmail), userManager, signInManager);
        var model = CreateModel(provider, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Account.ForgotPassword.IndexInputModel
        {
            EmailAddress = "alice",
            ClientId = "test-client-id"
        };

        var result = await model.OnPost(CancellationToken.None);
        var messagingClient = provider.GetRequiredService<IMessagingServiceClient>().ShouldBeOfType<TestMessagingServiceClient>();

        result.ShouldBeOfType<PageResult>();
        model.View.UserMessage.ShouldBe("Password Reset sent, please check your registered email for further instructions.");
        userManager.Verify(instance => instance.FindByNameAsync("alice"), Times.Once);
        userManager.Verify(instance => instance.GeneratePasswordResetTokenAsync(user), Times.Once);
        messagingClient.LastEmailRequest.ShouldNotBeNull();
        messagingClient.LastEmailRequest.Subject.ShouldBe("Password Reset Requested");
        messagingClient.LastEmailRequest.ToAddresses.ShouldContain("alice@example.com");
        messagingClient.LastEmailRequest.Body.ShouldContain("/Account/ResetPassword?userName=alice&resetToken=token%2B%2F%3D&clientId=test-client-id");
    }

    [Fact]
    public async Task OnPost_WhenUserIsMissing_ReturnsPageWithGenericMessageAndDoesNotSendEmail()
    {
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync((ApplicationUser?)null);

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = CreateProvider(nameof(this.OnPost_WhenUserIsMissing_ReturnsPageWithGenericMessageAndDoesNotSendEmail), userManager, signInManager);
        var model = CreateModel(provider, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Account.ForgotPassword.IndexInputModel
        {
            EmailAddress = "alice",
            ClientId = "test-client-id"
        };

        var result = await model.OnPost(CancellationToken.None);
        var messagingClient = provider.GetRequiredService<IMessagingServiceClient>().ShouldBeOfType<TestMessagingServiceClient>();

        result.ShouldBeOfType<PageResult>();
        model.View.UserMessage.ShouldBe("Password Reset sent, please check your registered email for further instructions.");
        userManager.Verify(instance => instance.FindByNameAsync("alice"), Times.Once);
        userManager.Verify(instance => instance.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        messagingClient.LastEmailRequest.ShouldBeNull();
    }

    private static ServiceProvider CreateProvider(string databaseName,
                                                  Mock<UserManager<ApplicationUser>> userManager,
                                                  Mock<SignInManager<ApplicationUser>> signInManager)
    {
        return TestServiceProviderFactory.Create(databaseName, userManager.Object, signInManager.Object);
    }

    private static SecurityService.Pages.Account.ForgotPassword.IndexModel CreateModel(ServiceProvider provider, HttpContext httpContext)
    {
        return new SecurityService.Pages.Account.ForgotPassword.IndexModel(provider.GetRequiredService<IMediator>())
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }
}
