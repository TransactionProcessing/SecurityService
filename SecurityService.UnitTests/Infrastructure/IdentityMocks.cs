using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SecurityService.Database;

namespace SecurityService.UnitTests.Infrastructure;

internal static class IdentityMocks
{
    public static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        var store = new Mock<IUserPasswordStore<ApplicationUser>>();
        var options = Options.Create(new IdentityOptions());
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = Array.Empty<IUserValidator<ApplicationUser>>();
        var passwordValidators = Array.Empty<IPasswordValidator<ApplicationUser>>();
        var keyNormalizer = new UpperInvariantLookupNormalizer();
        var errors = new IdentityErrorDescriber();
        var services = new ServiceCollection().BuildServiceProvider();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            options,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger.Object);
    }

    public static Mock<SignInManager<ApplicationUser>> CreateSignInManager(Mock<UserManager<ApplicationUser>>? userManager = null)
    {
        userManager ??= CreateUserManager();
        var contextAccessor = new Mock<IHttpContextAccessor>();
        contextAccessor.Setup(accessor => accessor.HttpContext).Returns(new DefaultHttpContext());
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var options = Options.Create(new IdentityOptions());
        var logger = new Mock<ILogger<SignInManager<ApplicationUser>>>();
        var schemes = new Mock<IAuthenticationSchemeProvider>();
        var confirmation = new Mock<IUserConfirmation<ApplicationUser>>();

        return new Mock<SignInManager<ApplicationUser>>(userManager.Object, contextAccessor.Object, claimsFactory.Object, options, logger.Object, schemes.Object, confirmation.Object);
    }
}

//internal sealed class FakeEmailService : IEmailService
//{
//    public List<AccountMessage> Messages { get; } = [];

//    public Task SendAsync(AccountMessage message, CancellationToken cancellationToken)
//    {
//        this.Messages.Add(message);
//        return Task.CompletedTask;
//    }
//}

internal sealed class FixedUrlHelper : IUrlHelper
{
    private readonly string _url;

    public FixedUrlHelper(string url)
    {
        this._url = url;
    }

    public ActionContext ActionContext { get; } = new(new DefaultHttpContext(), new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

    public string? Action(UrlActionContext actionContext) => this._url;

    public string? Content(string? contentPath) => this._url;

    public bool IsLocalUrl(string? url) => true;

    public string? Link(string? routeName, object? values) => this._url;

    public string? RouteUrl(UrlRouteContext routeContext) => this._url;
}
