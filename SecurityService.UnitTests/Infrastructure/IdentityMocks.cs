using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Imposter.Abstractions;
using SecurityService.Database.Entities;

namespace SecurityService.UnitTests.Infrastructure;

internal static class IdentityMocks
{
    public static UserManagerDouble CreateUserManager()
    {
        var store = new IUserPasswordStoreImposter<ApplicationUser>();
        var options = Options.Create(new IdentityOptions());
        var passwordHasher = new IPasswordHasherImposter<ApplicationUser>();
        var userValidators = Array.Empty<IUserValidator<ApplicationUser>>();
        var passwordValidators = Array.Empty<IPasswordValidator<ApplicationUser>>();
        var keyNormalizer = new UpperInvariantLookupNormalizer();
        var errors = new IdentityErrorDescriber();
        var services = new ServiceCollection().BuildServiceProvider();
        var logger = new ILoggerImposter<UserManager<ApplicationUser>>();

        return new UserManagerDouble(new TestUserManager(
            store.Instance(),
            options,
            passwordHasher.Instance(),
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger.Instance()));
    }

    public static SignInManagerDouble CreateSignInManager(UserManagerDouble? userManager = null)
    {
        userManager ??= CreateUserManager();
        var contextAccessor = new IHttpContextAccessorImposter();
        contextAccessor.HttpContext.Getter().Returns(new DefaultHttpContext());
        var claimsFactory = new IUserClaimsPrincipalFactoryImposter<ApplicationUser>();
        var options = Options.Create(new IdentityOptions());
        var logger = new ILoggerImposter<SignInManager<ApplicationUser>>();
        var schemes = new IAuthenticationSchemeProviderImposter();
        var confirmation = new IUserConfirmationImposter<ApplicationUser>();

        return new SignInManagerDouble(new SignInManager<ApplicationUser>(userManager.Instance(), contextAccessor.Instance(), claimsFactory.Instance(), options, logger.Instance(), schemes.Instance(), confirmation.Instance()));
    }
}

internal sealed class UserManagerDouble
{
    private readonly TestUserManager _instance;

    public UserManagerDouble(TestUserManager instance) => _instance = instance;

    public UserManager<ApplicationUser> Instance() => _instance;

    public Call<ApplicationUser?> FindByNameAsync(string _) => new(value => _instance.FindByNameResult = value, () => _instance.FindByNameCalls++);
    public Call<ApplicationUser?> FindByNameAsync(Arg<string> _) => FindByNameAsync(string.Empty);
    public Call<ApplicationUser?> FindByIdAsync(string _) => new(value => _instance.FindByIdResult = value, () => _instance.FindByIdCalls++);
    public Call<ApplicationUser?> FindByIdAsync(Arg<string> _) => FindByIdAsync(string.Empty);
    public Call<IList<string>> GetRolesAsync(ApplicationUser _) => new(value => _instance.RolesResult = value, () => _instance.GetRolesCalls++);
    public Call<IList<string>> GetRolesAsync(Arg<ApplicationUser> _) => GetRolesAsync(null!);
    public Call<IList<Claim>> GetClaimsAsync(ApplicationUser _) => new(value => _instance.ClaimsResult = value, () => _instance.GetClaimsCalls++);
    public Call<IList<Claim>> GetClaimsAsync(Arg<ApplicationUser> _) => GetClaimsAsync(null!);
    public Call<ApplicationUser?> GetUserAsync(ClaimsPrincipal _) => new(value => _instance.GetUserResult = value, () => _instance.GetUserCalls++);
    public Call<ApplicationUser?> GetUserAsync(Arg<ClaimsPrincipal> _) => GetUserAsync(null!);
    public Call<IdentityResult> ConfirmEmailAsync(ApplicationUser _, string __) => new(value => _instance.ConfirmEmailResult = value, () => _instance.ConfirmEmailCalls++);
    public Call<IdentityResult> ConfirmEmailAsync(ApplicationUser user, Arg<string> token) => ConfirmEmailAsync(user, string.Empty);
    public Call<IdentityResult> ConfirmEmailAsync(Arg<ApplicationUser> user, Arg<string> token) => ConfirmEmailAsync(null!, string.Empty);
    public Call<IdentityResult> RemovePasswordAsync(ApplicationUser _) => new(value => _instance.RemovePasswordResult = value, () => _instance.RemovePasswordCalls++);
    public Call<IdentityResult> RemovePasswordAsync(Arg<ApplicationUser> _) => RemovePasswordAsync(null!);
    public Call<IdentityResult> AddPasswordAsync(ApplicationUser _, string __) => new(value => _instance.AddPasswordResult = value, () => _instance.AddPasswordCalls++);
    public Call<IdentityResult> AddPasswordAsync(ApplicationUser user, Arg<string> password) => AddPasswordAsync(user, string.Empty);
    public Call<IdentityResult> AddPasswordAsync(Arg<ApplicationUser> user, Arg<string> password) => AddPasswordAsync(null!, string.Empty);
    public Call<string> GeneratePasswordResetTokenAsync(ApplicationUser _) => new(value => _instance.GenerateTokenResult = value, () => _instance.GenerateTokenCalls++);
    public Call<string> GeneratePasswordResetTokenAsync(Arg<ApplicationUser> _) => GeneratePasswordResetTokenAsync(null!);
    public Call<IdentityResult> ResetPasswordAsync(ApplicationUser _, string __, string ___) => new(value => _instance.ResetPasswordResult = value, () => _instance.ResetPasswordCalls++);
    public Call<IdentityResult> ResetPasswordAsync(Arg<ApplicationUser> user, Arg<string> token, Arg<string> password) => ResetPasswordAsync(null!, string.Empty, string.Empty);
}

internal sealed class SignInManagerDouble
{
    private readonly SignInManager<ApplicationUser> _instance;

    public SignInManagerDouble(SignInManager<ApplicationUser> instance) => _instance = instance;

    public SignInManager<ApplicationUser> Instance() => _instance;
}

internal sealed class Call<T>
{
    private readonly Action<T> _setResult;
    private readonly Action _recordSetup;

    public Call(Action<T> setResult, Action recordSetup)
    {
        _setResult = setResult;
        _recordSetup = recordSetup;
    }

    public Call<T> ReturnsAsync(T value)
    {
        _setResult(value);
        _recordSetup();
        return this;
    }

    public void Called(Count _) { }
}

internal sealed class TestUserManager : UserManager<ApplicationUser>
{
    public TestUserManager(
        IUserStore<ApplicationUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<ApplicationUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) { }

    public ApplicationUser? FindByNameResult { get; set; }
    public ApplicationUser? FindByIdResult { get; set; }
    public ApplicationUser? GetUserResult { get; set; }
    public IList<string> RolesResult { get; set; } = [];
    public IList<Claim> ClaimsResult { get; set; } = [];
    public IdentityResult ConfirmEmailResult { get; set; } = IdentityResult.Success;
    public IdentityResult RemovePasswordResult { get; set; } = IdentityResult.Success;
    public IdentityResult AddPasswordResult { get; set; } = IdentityResult.Success;
    public string GenerateTokenResult { get; set; } = string.Empty;
    public IdentityResult ResetPasswordResult { get; set; } = IdentityResult.Success;
    public int FindByNameCalls, FindByIdCalls, GetUserCalls, GetRolesCalls, GetClaimsCalls, ConfirmEmailCalls, RemovePasswordCalls, AddPasswordCalls, GenerateTokenCalls, ResetPasswordCalls;

    public override Task<ApplicationUser?> FindByNameAsync(string userName) { FindByNameCalls++; return Task.FromResult(FindByNameResult); }
    public override Task<ApplicationUser?> FindByIdAsync(string userId) { FindByIdCalls++; return Task.FromResult(FindByIdResult); }
    public override Task<IList<string>> GetRolesAsync(ApplicationUser user) { GetRolesCalls++; return Task.FromResult(RolesResult); }
    public override Task<IList<Claim>> GetClaimsAsync(ApplicationUser user) { GetClaimsCalls++; return Task.FromResult(ClaimsResult); }
    public override Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal) { GetUserCalls++; return Task.FromResult(GetUserResult); }
    public override Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token) { ConfirmEmailCalls++; return Task.FromResult(ConfirmEmailResult); }
    public override Task<IdentityResult> RemovePasswordAsync(ApplicationUser user) { RemovePasswordCalls++; return Task.FromResult(RemovePasswordResult); }
    public override Task<IdentityResult> AddPasswordAsync(ApplicationUser user, string password) { AddPasswordCalls++; return Task.FromResult(AddPasswordResult); }
    public override Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user) { GenerateTokenCalls++; return Task.FromResult(GenerateTokenResult); }
    public override Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword) { ResetPasswordCalls++; return Task.FromResult(ResetPasswordResult); }
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
