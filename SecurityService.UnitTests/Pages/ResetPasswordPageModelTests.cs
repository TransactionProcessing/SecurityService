using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SecurityService.Database;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;

namespace SecurityService.UnitTests.Pages;

public class ResetPasswordPageModelTests
{
    [Fact]
    public async Task OnGet_WithQueryValues_PopulatesInput()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = CreateProvider(nameof(this.OnGet_WithQueryValues_PopulatesInput), userManager, signInManager);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?userName=alice&resetToken=token-123&clientId=test-client-id");
        var model = CreateModel(provider, httpContext);

        var result = await model.OnGet();

        result.ShouldBeOfType<PageResult>();
        model.Input.Username.ShouldBe("alice");
        model.Input.Token.ShouldBe("token-123");
        model.Input.ClientId.ShouldBe("test-client-id");
    }

    [Fact]
    public async Task OnPost_WhenModelStateIsInvalid_ReturnsPageAndDoesNotCallMediatorPath()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = CreateProvider(nameof(this.OnPost_WhenModelStateIsInvalid_ReturnsPageAndDoesNotCallMediatorPath), userManager, signInManager);
        var model = CreateModel(provider, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Account.ResetPassword.ConfirmInputModel
        {
            Username = "alice",
            Token = "token-123",
            ClientId = "test-client-id",
            Password = "NewPassword1!",
            ConfirmPassword = "NewPassword1!"
        };
        model.ModelState.AddModelError("Password", "Required");

        var result = await model.OnPost(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        userManager.Verify(instance => instance.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_WhenPasswordsDoNotMatch_ReturnsPageAndDoesNotCallMediatorPath()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = CreateProvider(nameof(this.OnPost_WhenPasswordsDoNotMatch_ReturnsPageAndDoesNotCallMediatorPath), userManager, signInManager);
        var model = CreateModel(provider, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Account.ResetPassword.ConfirmInputModel
        {
            Username = "alice",
            Token = "token-123",
            ClientId = "test-client-id",
            Password = "NewPassword1!",
            ConfirmPassword = "DifferentPassword1!"
        };

        var result = await model.OnPost(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.ModelState[string.Empty]!.Errors.ShouldContain(error => error.ErrorMessage == "Password does not match Confirm Password");
        userManager.Verify(instance => instance.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_WhenMediatorPathFails_ReturnsPageWithFailureMessage()
    {
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync((ApplicationUser?)null);

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = CreateProvider(nameof(this.OnPost_WhenMediatorPathFails_ReturnsPageWithFailureMessage), userManager, signInManager);
        var model = CreateModel(provider, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Account.ResetPassword.ConfirmInputModel
        {
            Username = "alice",
            Token = "token-123",
            ClientId = "test-client-id",
            Password = "NewPassword1!",
            ConfirmPassword = "NewPassword1!"
        };

        var result = await model.OnPost(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.View.UserMessage.ShouldContain("Failed processing password reset for username alice");
        userManager.Verify(instance => instance.FindByNameAsync("alice"), Times.Once);
        userManager.Verify(instance => instance.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_WhenMediatorPathSucceeds_RedirectsToClientUri()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.ResetPasswordAsync(user, "token-123", "NewPassword1!"))
            .ReturnsAsync(IdentityResult.Success);

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = CreateProvider(nameof(this.OnPost_WhenMediatorPathSucceeds_RedirectsToClientUri), userManager, signInManager);
        await SeedClientAsync(provider, "test-client-id", "http://localhost/app");

        var model = CreateModel(provider, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Account.ResetPassword.ConfirmInputModel
        {
            Username = "alice",
            Token = "token-123",
            ClientId = "test-client-id",
            Password = "NewPassword1!",
            ConfirmPassword = "NewPassword1!"
        };

        var result = await model.OnPost(CancellationToken.None);

        var redirect = result.ShouldBeOfType<RedirectResult>();
        redirect.Url.ShouldBe("http://localhost/app");
        userManager.Verify(instance => instance.FindByNameAsync("alice"), Times.Once);
        userManager.Verify(instance => instance.ResetPasswordAsync(user, "token-123", "NewPassword1!"), Times.Once);
    }

    private static async Task SeedClientAsync(ServiceProvider provider, string clientId, string clientUri)
    {
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();
        dbContext.ClientDefinitions.Add(new ClientDefinition
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            ClientName = clientId,
            ClientUri = clientUri,
            ClientType = "confidential"
        });
        await dbContext.SaveChangesAsync();
    }

    private static ServiceProvider CreateProvider(string databaseName,
                                                  Mock<UserManager<ApplicationUser>> userManager,
                                                  Mock<SignInManager<ApplicationUser>> signInManager)
    {
        return TestServiceProviderFactory.Create(databaseName, userManager.Object, signInManager.Object);
    }

    private static SecurityService.Pages.Account.ResetPassword.IndexModel CreateModel(ServiceProvider provider, HttpContext httpContext)
    {
        return new SecurityService.Pages.Account.ResetPassword.IndexModel(provider.GetRequiredService<IMediator>())
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }
}
