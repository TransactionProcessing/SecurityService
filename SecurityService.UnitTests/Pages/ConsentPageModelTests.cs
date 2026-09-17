using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imposter.Abstractions;
using SecurityService.BusinessLogic.Oidc;
using Shouldly;
using SimpleResults;

namespace SecurityService.UnitTests.Pages;

public class ConsentPageModelTests
{
    [Fact]
    public async Task OnGetAsync_WhenHandlerReturnsInvalid_ReturnsBadRequest()
    {
        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result<ConsentGetQueryResult>>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success<ConsentGetQueryResult>(new ConsentGetInvalidResult("invalid")));

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Consent.IndexModel.InputModel { TransactionId = "token" };

        var result = await model.OnGetAsync("token", CancellationToken.None);

        var badRequest = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequest.Value.ShouldBe("invalid");
    }

    [Fact]
    public async Task OnGetAsync_WhenHandlerReturnsPage_SetsPropertiesAndReturnsPage()
    {
        var identityScopes = new[] { new ScopeDisplayItem("openid", "OpenID", null, true, false) };
        var apiScopes = new[] { new ScopeDisplayItem("api", "API", null, false, false) };
        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result<ConsentGetQueryResult>>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success<ConsentGetQueryResult>(new ConsentGetPageResult("My App", identityScopes, apiScopes)));

        var model = CreateModel(mediator, new DefaultHttpContext());

        var result = await model.OnGetAsync("token", CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.ClientName.ShouldBe("My App");
        model.IdentityScopes.Count.ShouldBe(1);
        model.ApiScopes.Count.ShouldBe(1);
    }

    [Fact]
    public async Task OnGetAsync_SendsQueryWithCorrectReturnUrl()
    {
        OidcCommands.ConsentGetQuery? capturedQuery = null;

        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result<ConsentGetQueryResult>>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success<ConsentGetQueryResult>(new ConsentGetPageResult(string.Empty, [], [])))
            .Callback((req, _) =>
            {
                capturedQuery = req.ShouldBeOfType<OidcCommands.ConsentGetQuery>();
                return default!;
            });

        var model = CreateModel(mediator, new DefaultHttpContext());
        await model.OnGetAsync("token", CancellationToken.None);

        capturedQuery.ShouldNotBeNull();
        capturedQuery.TransactionId.ShouldBe("token");
    }

    [Fact]
    public async Task OnPostAsync_WhenHandlerReturnsRedirect_ReturnsRedirectResult()
    {
        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result<ConsentPostCommandResult>>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success<ConsentPostCommandResult>(new ConsentPostRedirectResult("/return?consent=denied")));

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Consent.IndexModel.InputModel
        {
            TransactionId = "token",
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
        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result<ConsentPostCommandResult>>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success<ConsentPostCommandResult>(new ConsentPostPageResult("Select at least one scope to continue.")));

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Consent.IndexModel.InputModel
        {
            TransactionId = "token",
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

        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result<ConsentPostCommandResult>>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success<ConsentPostCommandResult>(new ConsentPostRedirectResult("/return?consent=accepted")))
            .Callback((req, _) =>
            {
                capturedCommand = req.ShouldBeOfType<OidcCommands.ConsentPostCommand>();
                return default!;
            });

        var model = CreateModel(mediator, new DefaultHttpContext());
        model.Input = new SecurityService.Pages.Consent.IndexModel.InputModel
        {
            TransactionId = "token",
            Button = "accept",
            SelectedScopes = ["openid", "profile"]
        };

        await model.OnPostAsync(CancellationToken.None);

        capturedCommand.ShouldNotBeNull();
        capturedCommand.TransactionId.ShouldBe("token");
        capturedCommand.Button.ShouldBe("accept");
        capturedCommand.SelectedScopes.ShouldContain("openid");
        capturedCommand.SelectedScopes.ShouldContain("profile");
    }

    private static SecurityService.Pages.Consent.IndexModel CreateModel(IMediatorImposter mediator, HttpContext httpContext)
    {
        return new SecurityService.Pages.Consent.IndexModel(mediator.Instance())
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }
}
