using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imposter.Abstractions;
using SecurityService.BusinessLogic.Mfa;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Models;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;
using SimpleResults;

namespace SecurityService.UnitTests.Pages;

public sealed class MfaLoginPageModelTests
{
    [Fact]
    public async Task Login_WhenMfaIsRequired_RedirectsToChallengeWithProtectedTransaction()
    {
        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result<List<ExternalProviderDetails>>> >.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(new List<ExternalProviderDetails>()));
        mediator.Send(Arg<IRequest<Result<LoginOutcome>>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success<LoginOutcome>(new LoginRequiresMfa("protected-transaction")));

        var model = new SecurityService.Pages.Account.Login.IndexModel(mediator.Instance())
        {
            Input = new SecurityService.Pages.Account.Login.IndexModel.InputModel
            {
                Username = "alice",
                Password = "password",
                ReturnUrl = "/connect/authorize?client_id=client"
            }
        };

        var result = await model.OnPostAsync();

        var redirect = result.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe("/Account/LoginWithMfa/Index");
        redirect.RouteValues!["transaction"].ShouldBe("protected-transaction");
    }

    [Fact]
    public async Task Challenge_WhenCodeIsInvalid_RedisplaysPageWithError()
    {
        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result<LoginOutcome>>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success<LoginOutcome>(new LoginRejected("The authenticator or recovery code was invalid.")));

        var model = new SecurityService.Pages.Account.LoginWithMfa.IndexModel(mediator.Instance())
        {
            Input = new SecurityService.Pages.Account.LoginWithMfa.IndexModel.InputModel
            {
                Transaction = "protected-transaction",
                Code = "000000"
            }
        };

        var result = await model.OnPostAsync();

        result.ShouldBeOfType<PageResult>();
        model.ModelState[string.Empty]!.Errors.Single().ErrorMessage.ShouldBe("The authenticator or recovery code was invalid.");
    }

    [Fact]
    public async Task Challenge_WhenCodeIsValid_RedirectsToProtectedReturnUrl()
    {
        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result<LoginOutcome>>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success<LoginOutcome>(new LoginCompleted("/connect/authorize?client_id=client")));

        var model = new SecurityService.Pages.Account.LoginWithMfa.IndexModel(mediator.Instance())
        {
            Input = new SecurityService.Pages.Account.LoginWithMfa.IndexModel.InputModel
            {
                Transaction = "protected-transaction",
                Code = "123456"
            }
        };

        var result = await model.OnPostAsync();

        var redirect = result.ShouldBeOfType<LocalRedirectResult>();
        redirect.Url.ShouldBe("/connect/authorize?client_id=client");
    }
}
