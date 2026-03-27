using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Models;
using Shouldly;
using SimpleResults;

namespace SecurityService.UnitTests.Pages;

public class ChangePasswordPageModelTests
{
    [Fact]
    public void OnGet_WithReturnUrlAndClientIdQuery_PopulatesInput()
    {
        var mediator = new Mock<IMediator>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?clientId=test-client-id");

        var model = CreateModel(mediator, httpContext);

        var result = model.OnGet("/connect/authorize?client_id=test-client-id");

        result.ShouldBeOfType<PageResult>();
        model.Input.ReturnUrl.ShouldBe("/connect/authorize?client_id=test-client-id");
        model.Input.ClientId.ShouldBe("test-client-id");
    }

    [Fact]
    public async Task OnPost_WhenPasswordsDoNotMatch_ReturnsPageAndDoesNotCallMediator()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Account.ChangePassword.IndexInputModel
        {
            EmailAddress = "alice",
            ReturnUrl = "/connect/authorize",
            ClientId = "test-client-id",
            CurrentPassword = "OldPassword1!",
            NewPassword = "NewPassword1!",
            ConfirmPassword = "DifferentPassword1!"
        };

        var result = await model.OnPost(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.ModelState[string.Empty]!.Errors.ShouldContain(error => error.ErrorMessage == "New Password does not match Confirm Password");
        mediator.Verify(instance => instance.Send(It.IsAny<SecurityServiceCommands.ChangeUserPasswordCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_WhenMediatorFails_ReturnsPageWithGenericError()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(instance => instance.Send(
                It.IsAny<SecurityServiceCommands.ChangeUserPasswordCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("boom"));

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Account.ChangePassword.IndexInputModel
        {
            EmailAddress = "alice",
            ReturnUrl = "/connect/authorize",
            ClientId = "test-client-id",
            CurrentPassword = "OldPassword1!",
            NewPassword = "NewPassword1!",
            ConfirmPassword = "NewPassword1!"
        };

        var result = await model.OnPost(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.ModelState[string.Empty]!.Errors.ShouldContain(error => error.ErrorMessage == "An error occurred changing password");
    }

    [Fact]
    public async Task OnPost_WhenMediatorSucceeds_RedirectsToReturnedUriAndSendsExpectedCommand()
    {
        var mediator = new Mock<IMediator>();
        SecurityServiceCommands.ChangeUserPasswordCommand? capturedCommand = null;
        mediator.Setup(instance => instance.Send(
                It.IsAny<SecurityServiceCommands.ChangeUserPasswordCommand>(),
                It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<ChangeUserPasswordResult>>, CancellationToken>((request, _) =>
            {
                capturedCommand = request.ShouldBeOfType<SecurityServiceCommands.ChangeUserPasswordCommand>();
            })
            .ReturnsAsync(Result.Success(new ChangeUserPasswordResult
            {
                IsSuccessful = true,
                RedirectUri = "http://localhost/app"
            }));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?clientId=query-client-id");
        httpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "alice")
        }, "TestAuthentication"));
        var model = CreateModel(mediator, httpContext);
        model.Input = new SecurityService.Pages.Account.ChangePassword.IndexInputModel
        {
            EmailAddress = "alice",
            ReturnUrl = "/connect/authorize?client_id=test-client-id",
            ClientId = "posted-client-id",
            CurrentPassword = "OldPassword1!",
            NewPassword = "NewPassword1!",
            ConfirmPassword = "NewPassword1!"
        };

        var result = await model.OnPost(CancellationToken.None);

        var redirect = result.ShouldBeOfType<RedirectResult>();
        redirect.Url.ShouldBe("http://localhost/app");
        capturedCommand.ShouldNotBeNull();
        capturedCommand.UserName.ShouldBe("alice");
        capturedCommand.CurrentPassword.ShouldBe("OldPassword1!");
        capturedCommand.NewPassword.ShouldBe("NewPassword1!");
        capturedCommand.ClientId.ShouldBe("query-client-id");
    }

    private static SecurityService.Pages.Account.ChangePassword.IndexModel CreateModel(Mock<IMediator> mediator, HttpContext httpContext)
    {
        return new SecurityService.Pages.Account.ChangePassword.IndexModel(mediator.Object)
        {
            PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
            {
                HttpContext = httpContext
            }
        };
    }
}
