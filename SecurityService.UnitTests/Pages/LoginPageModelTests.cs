using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SecurityService.Database;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;

namespace SecurityService.UnitTests.Pages;

public class LoginPageModelTests
{
    [Fact]
    public async Task OnPost_WhenForgotPasswordSelected_RedirectsToForgotPasswordWithReturnUrlAndClientId()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        var model = CreateModel(signInManager, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Account.Login.IndexModel.InputModel
        {
            Button = "forgotpassword",
            ReturnUrl = "/connect/authorize?client_id=test-client-id",
            ClientId = "test-client-id"
        };

        var result = await model.OnPostAsync();

        var redirect = result.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe("/Account/ForgotPassword/Index");
        redirect.RouteValues.ShouldNotBeNull();
        redirect.RouteValues["returnUrl"].ShouldBe("/connect/authorize?client_id=test-client-id");
        redirect.RouteValues["clientId"].ShouldBe("test-client-id");
        signInManager.Verify(instance => instance.PasswordSignInAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<bool>()), Times.Never);
    }

    private static SecurityService.Pages.Account.Login.IndexModel CreateModel(Mock<SignInManager<ApplicationUser>> signInManager, HttpContext httpContext)
    {
        return new SecurityService.Pages.Account.Login.IndexModel(signInManager.Object)
        {
            PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
            {
                HttpContext = httpContext
            }
        };
    }
}
