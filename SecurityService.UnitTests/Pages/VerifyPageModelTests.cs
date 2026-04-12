using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using SecurityService.BusinessLogic.Oidc;
using Shouldly;
using SimpleResults;

namespace SecurityService.UnitTests.Pages;

public class VerifyPageModelTests
{
    [Fact]
    public async Task OnGetAsync_WhenHandlerReturnsRedirect_ReturnsRedirectResult()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.VerifyGetQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<VerifyGetQueryResult>(new VerifyGetRedirectResult("/Account/Login?returnUrl=%2Fconnect%2Fverify")));

        var model = CreateModel(mediator, new DefaultHttpContext());

        var result = await model.OnGetAsync(CancellationToken.None);

        var redirect = result.ShouldBeOfType<RedirectResult>();
        redirect.Url.ShouldBe("/Account/Login?returnUrl=%2Fconnect%2Fverify");
    }

    [Fact]
    public async Task OnGetAsync_WhenHandlerReturnsPageWithStatusMessage_SetsStatusMessageAndReturnsPage()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.VerifyGetQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<VerifyGetQueryResult>(new VerifyGetPageResult("The specified user code is invalid.", null)));

        var model = CreateModel(mediator, new DefaultHttpContext());

        var result = await model.OnGetAsync(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.StatusMessage.ShouldBe("The specified user code is invalid.");
        model.ClientName.ShouldBe(string.Empty);
        model.RequestedScopes.ShouldBeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WhenHandlerReturnsPageWithDisplayData_AppliesDisplayDataAndReturnsPage()
    {
        var displayData = new VerifyDisplayData(
            "My App",
            ["openid", "profile"],
            [new ScopeDisplayItem("openid", "OpenID", null, true, false)],
            [],
            "ABCD-1234");

        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.VerifyGetQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<VerifyGetQueryResult>(new VerifyGetPageResult(string.Empty, displayData)));

        var model = CreateModel(mediator, new DefaultHttpContext());

        var result = await model.OnGetAsync(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.StatusMessage.ShouldBe(string.Empty);
        model.ClientName.ShouldBe("My App");
        model.RequestedScopes.ShouldBe(["openid", "profile"]);
        model.IdentityScopes.Count.ShouldBe(1);
        model.Input.UserCode.ShouldBe("ABCD-1234");
    }

    [Fact]
    public async Task OnGetAsync_SendsQueryWithCorrectHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        OidcCommands.VerifyGetQuery? capturedQuery = null;

        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.VerifyGetQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<VerifyGetQueryResult>>, CancellationToken>((req, _) =>
            {
                capturedQuery = req.ShouldBeOfType<OidcCommands.VerifyGetQuery>();
            })
            .ReturnsAsync(Result.Success<VerifyGetQueryResult>(new VerifyGetPageResult(string.Empty, null)));

        var model = CreateModel(mediator, httpContext);
        await model.OnGetAsync(CancellationToken.None);

        capturedQuery.ShouldNotBeNull();
        capturedQuery.HttpContext.ShouldBe(httpContext);
    }

    [Fact]
    public async Task OnPostAsync_WhenHandlerReturnsRedirect_ReturnsRedirectResult()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.VerifyPostCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<VerifyPostCommandResult>(new VerifyPostRedirectResult("/connect/verify?user_code=ABCD-1234")));

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Connect.VerifyModel.InputModel
        {
            Action = "lookup",
            UserCode = "ABCD-1234"
        };

        var result = await model.OnPostAsync(CancellationToken.None);

        var redirect = result.ShouldBeOfType<RedirectResult>();
        redirect.Url.ShouldBe("/connect/verify?user_code=ABCD-1234");
    }

    [Fact]
    public async Task OnPostAsync_WhenHandlerReturnsForbid_ReturnsForbidResult()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.VerifyPostCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<VerifyPostCommandResult>(new VerifyPostForbidResult("OpenIddict.Server.AspNetCore")));

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Connect.VerifyModel.InputModel { Action = "deny" };

        var result = await model.OnPostAsync(CancellationToken.None);

        result.ShouldBeOfType<ForbidResult>();
    }

    [Fact]
    public async Task OnPostAsync_WhenHandlerReturnsSignIn_ReturnsSignInResult()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var properties = new AuthenticationProperties { RedirectUri = "/" };

        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.VerifyPostCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<VerifyPostCommandResult>(new VerifyPostSignInResult(principal, properties, "OpenIddict.Server.AspNetCore")));

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Connect.VerifyModel.InputModel { Action = "authorize" };

        var result = await model.OnPostAsync(CancellationToken.None);

        var signIn = result.ShouldBeOfType<SignInResult>();
        signIn.Principal.ShouldBe(principal);
        signIn.Properties.ShouldBe(properties);
    }

    [Fact]
    public async Task OnPostAsync_WhenHandlerReturnsPageWithError_AddsModelErrorAndReturnsPage()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.VerifyPostCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<VerifyPostCommandResult>(new VerifyPostPageResult("Enter the user code shown on the device.", null)));

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Connect.VerifyModel.InputModel { Action = "lookup", UserCode = string.Empty };

        var result = await model.OnPostAsync(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.ModelState[string.Empty]!.Errors.ShouldContain(e => e.ErrorMessage == "Enter the user code shown on the device.");
    }

    [Fact]
    public async Task OnPostAsync_SendsCommandWithCorrectActionAndUserCode()
    {
        OidcCommands.VerifyPostCommand? capturedCommand = null;

        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.VerifyPostCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<VerifyPostCommandResult>>, CancellationToken>((req, _) =>
            {
                capturedCommand = req.ShouldBeOfType<OidcCommands.VerifyPostCommand>();
            })
            .ReturnsAsync(Result.Success<VerifyPostCommandResult>(new VerifyPostRedirectResult("/connect/verify?user_code=ABCD-1234")));

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Connect.VerifyModel.InputModel
        {
            Action = "lookup",
            UserCode = "ABCD-1234"
        };

        await model.OnPostAsync(CancellationToken.None);

        capturedCommand.ShouldNotBeNull();
        capturedCommand.Action.ShouldBe("lookup");
        capturedCommand.UserCode.ShouldBe("ABCD-1234");
    }

    private static SecurityService.Pages.Connect.VerifyModel CreateModel(Mock<IMediator> mediator, HttpContext httpContext)
    {
        return new SecurityService.Pages.Connect.VerifyModel(mediator.Object)
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }
}
