using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Imposter.Abstractions;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Models;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;
using SimpleResults;

namespace SecurityService.UnitTests.Pages;

public class LoginPageModelTests
{
    [Fact]
    public async Task OnPost_WhenForgotPasswordSelected_RedirectsToForgotPasswordWithReturnUrlAndClientId()
    {
        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result<List<ExternalProviderDetails>>>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(new List<ExternalProviderDetails>()));

        var model = CreateModel(mediator, new DefaultHttpContext());
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
        mediator.Send(Arg<SecurityServiceCommands.LoginCommand>.Any(), Arg<CancellationToken>.Any()).Called(Count.Never());
    }

    private static SecurityService.Pages.Account.Login.IndexModel CreateModel(IMediatorImposter mediator, HttpContext httpContext)
    {
        return new SecurityService.Pages.Account.Login.IndexModel(mediator.Instance())
        {
            PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
            {
                HttpContext = httpContext
            }
        };
    }
}
