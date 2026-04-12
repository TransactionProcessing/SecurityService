using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using SecurityService.BusinessLogic.Oidc;
using Shouldly;
using SimpleResults;

namespace SecurityService.UnitTests.Pages;

public class ConsentPageModelTests
{
    [Fact]
    public async Task OnGetAsync_WhenHandlerReturnsLocalRedirect_ReturnsLocalRedirectResult()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.ConsentGetQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<ConsentGetQueryResult>(new ConsentGetLocalRedirectResult("/return")));

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Consent.IndexModel.InputModel { ReturnUrl = "/return" };

        var result = await model.OnGetAsync("/return", CancellationToken.None);

        var redirect = result.ShouldBeOfType<LocalRedirectResult>();
        redirect.Url.ShouldBe("/return");
    }

    [Fact]
    public async Task OnGetAsync_WhenHandlerReturnsPage_SetsPropertiesAndReturnsPage()
    {
        var identityScopes = new[] { new ScopeDisplayItem("openid", "OpenID", null, true, false) };
        var apiScopes = new[] { new ScopeDisplayItem("api", "API", null, false, false) };
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.ConsentGetQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<ConsentGetQueryResult>(new ConsentGetPageResult("My App", identityScopes, apiScopes)));

        var model = CreateModel(mediator, new DefaultHttpContext());

        var result = await model.OnGetAsync("/return", CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.ClientName.ShouldBe("My App");
        model.IdentityScopes.Count.ShouldBe(1);
        model.ApiScopes.Count.ShouldBe(1);
    }

    [Fact]
    public async Task OnGetAsync_SendsQueryWithCorrectReturnUrl()
    {
        OidcCommands.ConsentGetQuery? capturedQuery = null;

        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.ConsentGetQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<ConsentGetQueryResult>>, CancellationToken>((req, _) =>
            {
                capturedQuery = req.ShouldBeOfType<OidcCommands.ConsentGetQuery>();
            })
            .ReturnsAsync(Result.Success<ConsentGetQueryResult>(new ConsentGetPageResult(string.Empty, [], [])));

        var model = CreateModel(mediator, new DefaultHttpContext());
        await model.OnGetAsync("/my-return", CancellationToken.None);

        capturedQuery.ShouldNotBeNull();
        capturedQuery.ReturnUrl.ShouldBe("/my-return");
    }

    [Fact]
    public async Task OnPostAsync_WhenHandlerReturnsRedirect_ReturnsRedirectResult()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.ConsentPostCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<ConsentPostCommandResult>(new ConsentPostRedirectResult("/return?consent=denied")));

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Consent.IndexModel.InputModel
        {
            ReturnUrl = "/return",
            Button = "deny",
            SelectedScopes = []
        };

        var result = await model.OnPostAsync(CancellationToken.None);

        var redirect = result.ShouldBeOfType<RedirectResult>();
        redirect.Url.ShouldBe("/return?consent=denied");
    }

    [Fact]
    public async Task OnPostAsync_WhenHandlerReturnsPageWithError_AddsModelErrorAndReturnsPage()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.ConsentPostCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<ConsentPostCommandResult>(new ConsentPostPageResult("Select at least one scope to continue.")));

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Consent.IndexModel.InputModel
        {
            ReturnUrl = "/return",
            Button = "accept",
            SelectedScopes = []
        };

        var result = await model.OnPostAsync(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.ModelState[string.Empty]!.Errors.ShouldContain(e => e.ErrorMessage == "Select at least one scope to continue.");
    }

    [Fact]
    public async Task OnPostAsync_SendsCommandWithCorrectValues()
    {
        OidcCommands.ConsentPostCommand? capturedCommand = null;

        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<OidcCommands.ConsentPostCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<ConsentPostCommandResult>>, CancellationToken>((req, _) =>
            {
                capturedCommand = req.ShouldBeOfType<OidcCommands.ConsentPostCommand>();
            })
            .ReturnsAsync(Result.Success<ConsentPostCommandResult>(new ConsentPostRedirectResult("/return?consent=accepted")));

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Consent.IndexModel.InputModel
        {
            ReturnUrl = "/return",
            Button = "accept",
            SelectedScopes = ["openid", "profile"]
        };

        await model.OnPostAsync(CancellationToken.None);

        capturedCommand.ShouldNotBeNull();
        capturedCommand.ReturnUrl.ShouldBe("/return");
        capturedCommand.Button.ShouldBe("accept");
        capturedCommand.SelectedScopes.ShouldContain("openid");
        capturedCommand.SelectedScopes.ShouldContain("profile");
    }

    private static SecurityService.Pages.Consent.IndexModel CreateModel(Mock<IMediator> mediator, HttpContext httpContext)
    {
        return new SecurityService.Pages.Consent.IndexModel(mediator.Object)
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }
}
