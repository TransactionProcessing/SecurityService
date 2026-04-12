using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database;
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
        userManager.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser?)null);

        var mediator = new Mock<IMediator>();
        var model = CreateModel(userManager, mediator, new DefaultHttpContext());

        var result = await model.OnGetAsync(CancellationToken.None);

        result.ShouldBeOfType<RedirectResult>();
    }

    [Fact]
    public async Task OnGetAsync_WhenUserFound_QueriesGrantsAndReturnsPage()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var grants = new List<GrantDetails>
        {
            new GrantDetails("auth-1", "client-1", "Client One", new[] { "openid" }, DateTimeOffset.UtcNow)
        };

        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.Is<SecurityServiceQueries.GetUserGrantsQuery>(q => q.UserId == "user-1"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(grants));

        var model = CreateModel(userManager, mediator, new DefaultHttpContext());

        var result = await model.OnGetAsync(CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.Grants.ShouldHaveSingleItem();
        mediator.Verify(m => m.Send(It.Is<SecurityServiceQueries.GetUserGrantsQuery>(q => q.UserId == "user-1"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnPostRevokeAsync_WhenUserNotFound_RedirectsToLogin()
    {
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser?)null);

        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var model = CreateModel(userManager, mediator, new DefaultHttpContext());

        var result = await model.OnPostRevokeAsync("auth-1", CancellationToken.None);

        result.ShouldBeOfType<RedirectResult>();
        mediator.Verify(m => m.Send(It.IsAny<SecurityServiceCommands.RevokeGrantCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OnPostRevokeAsync_WhenRevokeSucceeds_RedirectsToPage()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<SecurityServiceCommands.RevokeGrantCommand>(), It.IsAny<CancellationToken>()))
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
        userManager.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<SecurityServiceCommands.RevokeGrantCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("The authorization could not be revoked."));
        mediator.Setup(m => m.Send(It.IsAny<SecurityServiceQueries.GetUserGrantsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<GrantDetails>()));

        var model = CreateModel(userManager, mediator, new DefaultHttpContext());

        var result = await model.OnPostRevokeAsync("auth-1", CancellationToken.None);

        result.ShouldBeOfType<PageResult>();
        model.StatusMessage.ShouldBe("The authorization could not be revoked.");
    }

    private static SecurityService.Pages.Account.Grants.IndexModel CreateModel(
        Mock<UserManager<ApplicationUser>> userManager,
        Mock<IMediator> mediator,
        HttpContext httpContext)
    {
        return new SecurityService.Pages.Account.Grants.IndexModel(userManager.Object, mediator.Object)
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }
}
