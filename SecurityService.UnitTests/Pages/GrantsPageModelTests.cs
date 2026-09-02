using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imposter.Abstractions;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database.Entities;
using SecurityService.Models;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;
using SimpleResults;

namespace SecurityService.UnitTests.Pages;

public class GrantsPageModelTests
{
    [Fact]
    public async Task OnGetAsync_WhenUserNotFound_RedirectsToLogin()
    {
        var userManager = IdentityMocks.CreateUserManager();
        userManager.GetUserAsync(Arg<System.Security.Claims.ClaimsPrincipal>.Any())
            .ReturnsAsync((ApplicationUser?)null);

        var mediator = new IMediatorImposter();
        var model = CreateModel(userManager, mediator, new DefaultHttpContext());

        var result = await model.OnGetAsync(CancellationToken.None);

        result.ShouldBeOfType<RedirectResult>();
    }

    [Fact]
    public async Task OnGetAsync_WhenUserFound_QueriesGrantsAndReturnsPage()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.GetUserAsync(Arg<System.Security.Claims.ClaimsPrincipal>.Any())
            .ReturnsAsync(user);

        var grants = new List<GrantDetails>
        {
            new GrantDetails("auth-1", "client-1", "Client One", new[] { "openid" }, DateTimeOffset.UtcNow)
        };

        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result<List<GrantDetails>>>>.Is(q => q is SecurityServiceQueries.GetUserGrantsQuery query && query.UserId == "user-1"), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(grants));

        var model = CreateModel(userManager, mediator, new DefaultHttpContext());

        var result = await model.OnGetAsync(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.Grants.ShouldHaveSingleItem();
        mediator.Send(Arg<IRequest<Result<List<GrantDetails>>>>.Is(q => q is SecurityServiceQueries.GetUserGrantsQuery query && query.UserId == "user-1"), Arg<CancellationToken>.Any()).Called(Count.Once());
    }

    [Fact]
    public async Task OnPostRevokeAsync_WhenUserNotFound_RedirectsToLogin()
    {
        var userManager = IdentityMocks.CreateUserManager();
        userManager.GetUserAsync(Arg<System.Security.Claims.ClaimsPrincipal>.Any())
            .ReturnsAsync((ApplicationUser?)null);

        var mediator = new IMediatorImposter(ImposterMode.Explicit);
        var model = CreateModel(userManager, mediator, new DefaultHttpContext());

        var result = await model.OnPostRevokeAsync("auth-1", CancellationToken.None);

        result.ShouldBeOfType<RedirectResult>();
        mediator.Send(Arg<SecurityServiceCommands.RevokeGrantCommand>.Any(), Arg<CancellationToken>.Any()).Called(Count.Never());
    }

    [Fact]
    public async Task OnPostRevokeAsync_WhenRevokeSucceeds_RedirectsToPage()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.GetUserAsync(Arg<System.Security.Claims.ClaimsPrincipal>.Any())
            .ReturnsAsync(user);

        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        var model = CreateModel(userManager, mediator, new DefaultHttpContext());

        var result = await model.OnPostRevokeAsync("auth-1", CancellationToken.None);

        result.ShouldBeOfType<RedirectToPageResult>();
    }

    [Fact]
    public async Task OnPostRevokeAsync_WhenRevokeFails_ReturnsPageWithStatusMessage()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.GetUserAsync(Arg<System.Security.Claims.ClaimsPrincipal>.Any())
            .ReturnsAsync(user);

        var mediator = new IMediatorImposter();
        mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure("The authorization could not be revoked."));
        mediator.Send(Arg<IRequest<Result<List<GrantDetails>>>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(new List<GrantDetails>()));

        var model = CreateModel(userManager, mediator, new DefaultHttpContext());

        var result = await model.OnPostRevokeAsync("auth-1", CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.StatusMessage.ShouldBe("The authorization could not be revoked.");
    }

    private static SecurityService.Pages.Account.Grants.IndexModel CreateModel(
        UserManagerDouble userManager,
        IMediatorImposter mediator,
        HttpContext httpContext)
    {
        return new SecurityService.Pages.Account.Grants.IndexModel(userManager.Instance(), mediator.Instance())
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }
}
