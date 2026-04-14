using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenIddict.Server.AspNetCore;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.BusinessLogic.RequestHandlers;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;

namespace SecurityService.UnitTests.RequestHandlers;

public class VerifyRequestHandlerTests
{
    [Fact]
    public async Task VerifyGetQuery_WhenAuthFails_ReturnsPageResultWithNoData()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var scopeManager = new Mock<OpenIddict.Abstractions.IOpenIddictScopeManager>();
        var appManager = new Mock<OpenIddict.Abstractions.IOpenIddictApplicationManager>();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(this.VerifyGetQuery_WhenAuthFails_ReturnsPageResultWithNoData));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var handler = new VerifyRequestHandler(appManager.Object, scopeManager.Object, dbContext, userManager.Object);
        var context = CreateHttpContext(AuthenticateResult.Fail("no ticket"), isAuthenticated: false, userCode: string.Empty);

        var result = await handler.Handle(new OidcCommands.VerifyGetQuery(context), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var page = result.Data.ShouldBeOfType<VerifyGetPageResult>();
        page.StatusMessage.ShouldBe(string.Empty);
        page.Data.ShouldBeNull();
    }

    [Fact]
    public async Task VerifyGetQuery_WhenAuthFailsWithUserCode_ReturnsPageWithInvalidCodeMessage()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var scopeManager = new Mock<OpenIddict.Abstractions.IOpenIddictScopeManager>();
        var appManager = new Mock<OpenIddict.Abstractions.IOpenIddictApplicationManager>();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(this.VerifyGetQuery_WhenAuthFailsWithUserCode_ReturnsPageWithInvalidCodeMessage));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var handler = new VerifyRequestHandler(appManager.Object, scopeManager.Object, dbContext, userManager.Object);
        var context = CreateHttpContext(AuthenticateResult.Fail("no ticket"), isAuthenticated: false, userCode: "ABCD-1234");

        var result = await handler.Handle(new OidcCommands.VerifyGetQuery(context), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var page = result.Data.ShouldBeOfType<VerifyGetPageResult>();
        page.StatusMessage.ShouldBe("The specified user code is invalid.");
        page.Data.ShouldBeNull();
    }

    [Fact]
    public async Task VerifyGetQuery_WhenAuthSucceedsButUserNotAuthenticated_ReturnsRedirectToLogin()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var scopeManager = new Mock<OpenIddict.Abstractions.IOpenIddictScopeManager>();
        var appManager = new Mock<OpenIddict.Abstractions.IOpenIddictApplicationManager>();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(this.VerifyGetQuery_WhenAuthSucceedsButUserNotAuthenticated_ReturnsRedirectToLogin));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var authTicket = CreateAuthTicket("client-1");
        var handler = new VerifyRequestHandler(appManager.Object, scopeManager.Object, dbContext, userManager.Object);
        var context = CreateHttpContext(AuthenticateResult.Success(authTicket), isAuthenticated: false, userCode: string.Empty);

        var result = await handler.Handle(new OidcCommands.VerifyGetQuery(context), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var redirect = result.Data.ShouldBeOfType<VerifyGetRedirectResult>();
        redirect.Url.ShouldContain("/Account/Login");
    }

    [Fact]
    public async Task VerifyPostCommand_WithLookupAction_WhenEmptyUserCode_ReturnsPageWithError()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var scopeManager = new Mock<OpenIddict.Abstractions.IOpenIddictScopeManager>();
        var appManager = new Mock<OpenIddict.Abstractions.IOpenIddictApplicationManager>();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(this.VerifyPostCommand_WithLookupAction_WhenEmptyUserCode_ReturnsPageWithError));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var handler = new VerifyRequestHandler(appManager.Object, scopeManager.Object, dbContext, userManager.Object);
        var context = CreateHttpContext(AuthenticateResult.Fail("irrelevant"), isAuthenticated: false, userCode: string.Empty);

        var result = await handler.Handle(
            new OidcCommands.VerifyPostCommand(context, "lookup", string.Empty),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var page = result.Data.ShouldBeOfType<VerifyPostPageResult>();
        page.ModelError.ShouldBe("Enter the user code shown on the device.");
    }

    [Fact]
    public async Task VerifyPostCommand_WithLookupAction_WhenValidUserCode_ReturnsRedirect()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var scopeManager = new Mock<OpenIddict.Abstractions.IOpenIddictScopeManager>();
        var appManager = new Mock<OpenIddict.Abstractions.IOpenIddictApplicationManager>();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(this.VerifyPostCommand_WithLookupAction_WhenValidUserCode_ReturnsRedirect));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var handler = new VerifyRequestHandler(appManager.Object, scopeManager.Object, dbContext, userManager.Object);
        var context = CreateHttpContext(AuthenticateResult.Fail("irrelevant"), isAuthenticated: false, userCode: string.Empty);

        var result = await handler.Handle(
            new OidcCommands.VerifyPostCommand(context, "lookup", "ABCD-1234"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var redirect = result.Data.ShouldBeOfType<VerifyPostRedirectResult>();
        redirect.Url.ShouldBe("/connect/verify?user_code=ABCD-1234");
    }

    [Fact]
    public async Task VerifyPostCommand_WhenAuthFails_ReturnsPageWithInvalidCodeError()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var scopeManager = new Mock<OpenIddict.Abstractions.IOpenIddictScopeManager>();
        var appManager = new Mock<OpenIddict.Abstractions.IOpenIddictApplicationManager>();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(this.VerifyPostCommand_WhenAuthFails_ReturnsPageWithInvalidCodeError));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var handler = new VerifyRequestHandler(appManager.Object, scopeManager.Object, dbContext, userManager.Object);
        var context = CreateHttpContext(AuthenticateResult.Fail("no ticket"), isAuthenticated: false, userCode: string.Empty);

        var result = await handler.Handle(
            new OidcCommands.VerifyPostCommand(context, "authorize", "ABCD-1234"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var page = result.Data.ShouldBeOfType<VerifyPostPageResult>();
        page.ModelError.ShouldBe("The specified user code is invalid.");
    }

    [Fact]
    public async Task VerifyPostCommand_WhenAuthSucceedsButUserNotAuthenticated_ReturnsRedirectToLogin()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var scopeManager = new Mock<OpenIddict.Abstractions.IOpenIddictScopeManager>();
        var appManager = new Mock<OpenIddict.Abstractions.IOpenIddictApplicationManager>();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(this.VerifyPostCommand_WhenAuthSucceedsButUserNotAuthenticated_ReturnsRedirectToLogin));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var authTicket = CreateAuthTicket("client-1");
        var handler = new VerifyRequestHandler(appManager.Object, scopeManager.Object, dbContext, userManager.Object);
        var context = CreateHttpContext(AuthenticateResult.Success(authTicket), isAuthenticated: false, userCode: string.Empty);

        var result = await handler.Handle(
            new OidcCommands.VerifyPostCommand(context, "authorize", "ABCD-1234"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var redirect = result.Data.ShouldBeOfType<VerifyPostRedirectResult>();
        redirect.Url.ShouldContain("/Account/Login");
    }

    [Fact]
    public async Task VerifyPostCommand_WithDenyAction_WhenAuthenticatedUser_ReturnsForbid()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var scopeManager = new Mock<OpenIddict.Abstractions.IOpenIddictScopeManager>();
        var appManager = new Mock<OpenIddict.Abstractions.IOpenIddictApplicationManager>();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(this.VerifyPostCommand_WithDenyAction_WhenAuthenticatedUser_ReturnsForbid));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var authTicket = CreateAuthTicket("client-1");
        var handler = new VerifyRequestHandler(appManager.Object, scopeManager.Object, dbContext, userManager.Object);
        var context = CreateHttpContext(AuthenticateResult.Success(authTicket), isAuthenticated: true, userCode: string.Empty);

        var result = await handler.Handle(
            new OidcCommands.VerifyPostCommand(context, "deny", "ABCD-1234"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var forbid = result.Data.ShouldBeOfType<VerifyPostForbidResult>();
        forbid.AuthenticationScheme.ShouldBe(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Fact]
    public async Task VerifyPostCommand_WhenAuthenticatedUserNotFoundInStore_ReturnsRedirectToLogin()
    {
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser?)null);
        var scopeManager = new Mock<OpenIddict.Abstractions.IOpenIddictScopeManager>();
        var appManager = new Mock<OpenIddict.Abstractions.IOpenIddictApplicationManager>();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(this.VerifyPostCommand_WhenAuthenticatedUserNotFoundInStore_ReturnsRedirectToLogin));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var authTicket = CreateAuthTicket("client-1");
        var handler = new VerifyRequestHandler(appManager.Object, scopeManager.Object, dbContext, userManager.Object);
        var context = CreateHttpContext(AuthenticateResult.Success(authTicket), isAuthenticated: true, userCode: string.Empty);

        var result = await handler.Handle(
            new OidcCommands.VerifyPostCommand(context, "authorize", "ABCD-1234"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var redirect = result.Data.ShouldBeOfType<VerifyPostRedirectResult>();
        redirect.Url.ShouldContain("/Account/Login");
    }

    private static HttpContext CreateHttpContext(AuthenticateResult authResult, bool isAuthenticated, string userCode)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IAuthenticationService>(new FakeAuthenticationService(authResult));

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        if (!string.IsNullOrEmpty(userCode))
        {
            context.Request.QueryString = new QueryString($"?user_code={Uri.EscapeDataString(userCode)}");
        }

        if (isAuthenticated)
        {
            var identity = new ClaimsIdentity("Test");
            identity.AddClaim(new Claim(ClaimTypes.Name, "testuser"));
            context.User = new ClaimsPrincipal(identity);
        }

        return context;
    }

    private static AuthenticationTicket CreateAuthTicket(string clientId)
    {
        var identity = new ClaimsIdentity("OpenIddict.Server.AspNetCore");
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.ClientId, clientId));
        var principal = new ClaimsPrincipal(identity);
        return new AuthenticationTicket(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        private readonly AuthenticateResult _authenticateResult;

        public FakeAuthenticationService(AuthenticateResult authenticateResult)
        {
            this._authenticateResult = authenticateResult;
        }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) =>
            Task.FromResult(this._authenticateResult);

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;
    }
}

