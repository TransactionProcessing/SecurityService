using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Imposter.Abstractions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Mfa;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Models;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;
using SimpleResults;

namespace SecurityService.UnitTests.Pages;

public class LoginPageModelTests
{
    [Fact]
    public async Task OnPost_WhenCancelSelected_RedirectsToHome()
    {
        var mediator = CreateMediator();
        var model = CreateModel(mediator);
        model.Input.Button = "cancel";

        var result = await model.OnPostAsync();

        result.ShouldBeOfType<LocalRedirectResult>().Url.ShouldBe("/");
        AssertLoginWasNotSent(mediator);
    }

    [Fact]
    public async Task OnPost_WhenForgotPasswordSelected_RedirectsWithReturnUrlAndClientId()
    {
        var mediator = CreateMediator();
        var model = CreateModel(mediator);
        model.Input = new SecurityService.Pages.Account.Login.IndexModel.InputModel
        {
            Button = "forgotpassword",
            ReturnUrl = "/connect/authorize?client_id=test-client-id",
            ClientId = "test-client-id"
        };

        var result = await model.OnPostAsync();

        var redirect = result.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe("/Account/ForgotPassword/Index");
        redirect.RouteValues!["returnUrl"].ShouldBe("/connect/authorize?client_id=test-client-id");
        redirect.RouteValues["clientId"].ShouldBe("test-client-id");
        AssertLoginWasNotSent(mediator);
    }

    [Fact]
    public async Task OnPost_WhenLoginRejected_RedisplaysPageWithRejectionMessage()
    {
        var mediator = CreateMediator(Result.Success<LoginOutcome>(new LoginRejected("The account is locked.")));
        var model = CreateModel(mediator);

        var result = await model.OnPostAsync();

        result.ShouldBeOfType<PageResult>();
        model.ModelState[string.Empty]!.Errors.Single().ErrorMessage.ShouldBe("The account is locked.");
    }

    [Fact]
    public async Task OnPost_WhenLoginFailsWithoutRejection_UsesResultErrorMessage()
    {
        var mediator = CreateMediator(Result.Failure("Login service unavailable."));
        var model = CreateModel(mediator);

        var result = await model.OnPostAsync();

        result.ShouldBeOfType<PageResult>();
        model.ModelState[string.Empty]!.Errors.Single().ErrorMessage.ShouldBe("Login service unavailable.");
    }

    [Fact]
    public async Task OnPost_WhenLoginCompletes_RedirectsToReturnUrl()
    {
        var mediator = CreateMediator(Result.Success<LoginOutcome>(new LoginCompleted("/connect/authorize")));
        var model = CreateModel(mediator);

        var result = await model.OnPostAsync();

        result.ShouldBeOfType<LocalRedirectResult>().Url.ShouldBe("/connect/authorize");
    }

    [Fact]
    public async Task OnPost_WhenCompletedReturnUrlIsEmpty_RedirectsToHome()
    {
        var mediator = CreateMediator(Result.Success<LoginOutcome>(new LoginCompleted(" ")));
        var model = CreateModel(mediator);

        var result = await model.OnPostAsync();

        result.ShouldBeOfType<LocalRedirectResult>().Url.ShouldBe("/");
    }

    [Fact]
    public async Task OnPost_WhenMfaIsRequired_RedirectsToChallengeWithTransaction()
    {
        var mediator = CreateMediator(Result.Success<LoginOutcome>(new LoginRequiresMfa("protected-transaction")));
        var model = CreateModel(mediator);

        var result = await model.OnPostAsync();

        var redirect = result.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe("/Account/LoginWithMfa/Index");
        redirect.RouteValues!["transaction"].ShouldBe("protected-transaction");
    }

    [Fact]
    public async Task OnPost_WhenMfaEnrollmentIsRequired_RedirectsToEnrollmentWithTransaction()
    {
        var mediator = CreateMediator(Result.Success<LoginOutcome>(new LoginRequiresMfaEnrollment("enrollment-transaction")));
        var model = CreateModel(mediator);

        var result = await model.OnPostAsync();

        var redirect = result.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe("/Account/EnrollMfa/Index");
        redirect.RouteValues!["transaction"].ShouldBe("enrollment-transaction");
    }

    [Fact]
    public async Task OnPost_WhenLoginOutcomeIsUnsupported_RedisplaysPageWithError()
    {
        var mediator = CreateMediator(Result.Success<LoginOutcome>(new UnsupportedLoginOutcome()));
        var model = CreateModel(mediator);

        var result = await model.OnPostAsync();

        result.ShouldBeOfType<PageResult>();
        model.ModelState[string.Empty]!.Errors.Single().ErrorMessage.ShouldBe("Unsupported Login Success Type");
    }

    private static IMediatorImposter CreateMediator(Result<LoginOutcome>? loginResult = null)
    {
        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result<List<ExternalProviderDetails>>>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(new List<ExternalProviderDetails>()));
        if (loginResult is not null)
        {
            mediator.Send(Arg<IRequest<Result<LoginOutcome>>>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(loginResult);
        }

        return mediator;
    }

    private static SecurityService.Pages.Account.Login.IndexModel CreateModel(IMediatorImposter mediator)
    {
        return new SecurityService.Pages.Account.Login.IndexModel(mediator.Instance())
        {
            PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static void AssertLoginWasNotSent(IMediatorImposter mediator)
    {
        mediator.Send(Arg<IRequest<Result<LoginOutcome>>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Never());
    }

    private sealed record UnsupportedLoginOutcome : LoginOutcome;
}
