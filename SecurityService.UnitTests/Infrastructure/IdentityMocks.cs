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
    private readonly List<CallSetup<string, ApplicationUser?>> _findByNameSetups = [];
    private readonly List<string> _findByNameCalls = [];
    private readonly List<CallSetup<string, ApplicationUser?>> _findByIdSetups = [];
    private readonly List<string> _findByIdCalls = [];
    private readonly List<CallSetup<ApplicationUser, IList<string>>> _getRolesSetups = [];
    private readonly List<ApplicationUser> _getRolesCalls = [];
    private readonly List<CallSetup<ApplicationUser, IList<Claim>>> _getClaimsSetups = [];
    private readonly List<ApplicationUser> _getClaimsCalls = [];
    private readonly List<CallSetup<ClaimsPrincipal, ApplicationUser?>> _getUserSetups = [];
    private readonly List<ClaimsPrincipal> _getUserCalls = [];
    private readonly List<CallSetup<(ApplicationUser User, string Token), IdentityResult>> _confirmEmailSetups = [];
    private readonly List<(ApplicationUser User, string Token)> _confirmEmailCalls = [];
    private readonly List<CallSetup<ApplicationUser, IdentityResult>> _removePasswordSetups = [];
    private readonly List<ApplicationUser> _removePasswordCalls = [];
    private readonly List<CallSetup<(ApplicationUser User, string Password), IdentityResult>> _addPasswordSetups = [];
    private readonly List<(ApplicationUser User, string Password)> _addPasswordCalls = [];
    private readonly List<CallSetup<ApplicationUser, string>> _generateTokenSetups = [];
    private readonly List<ApplicationUser> _generateTokenCalls = [];
    private readonly List<CallSetup<(ApplicationUser User, string Token, string Password), IdentityResult>> _resetPasswordSetups = [];
    private readonly List<(ApplicationUser User, string Token, string Password)> _resetPasswordCalls = [];
    private readonly TestUserManager _instance;

    public UserManagerDouble(TestUserManager instance)
    {
        _instance = instance;
        _instance.FindByName = userName => Resolve(_findByNameSetups, _findByNameCalls, userName);
        _instance.FindById = userId => Resolve(_findByIdSetups, _findByIdCalls, userId);
        _instance.GetRoles = user => Resolve(_getRolesSetups, _getRolesCalls, user) ?? [];
        _instance.GetClaims = user => Resolve(_getClaimsSetups, _getClaimsCalls, user) ?? [];
        _instance.GetUser = principal => Resolve(_getUserSetups, _getUserCalls, principal);
        _instance.ConfirmEmail = request => Resolve(_confirmEmailSetups, _confirmEmailCalls, request) ?? IdentityResult.Success;
        _instance.RemovePassword = user => Resolve(_removePasswordSetups, _removePasswordCalls, user) ?? IdentityResult.Success;
        _instance.AddPassword = request => Resolve(_addPasswordSetups, _addPasswordCalls, request) ?? IdentityResult.Success;
        _instance.GenerateToken = user => Resolve(_generateTokenSetups, _generateTokenCalls, user) ?? string.Empty;
        _instance.ResetPassword = request => Resolve(_resetPasswordSetups, _resetPasswordCalls, request) ?? IdentityResult.Success;
    }

    public UserManager<ApplicationUser> Instance() => _instance;

    public Call<string, ApplicationUser?> FindByNameAsync(string userName) => Add(_findByNameSetups, _findByNameCalls, actual => userName.Equals(actual, StringComparison.Ordinal));
    public Call<string, ApplicationUser?> FindByNameAsync(Arg<string> _) => Add(_findByNameSetups, _findByNameCalls, Any<string>());
    public Call<string, ApplicationUser?> FindByIdAsync(string userId) => Add(_findByIdSetups, _findByIdCalls, actual => userId.Equals(actual, StringComparison.Ordinal));
    public Call<string, ApplicationUser?> FindByIdAsync(Arg<string> _) => Add(_findByIdSetups, _findByIdCalls, Any<string>());
    public Call<ApplicationUser, IList<string>> GetRolesAsync(ApplicationUser user) => Add(_getRolesSetups, _getRolesCalls, actual => user.Equals(actual));
    public Call<ApplicationUser, IList<string>> GetRolesAsync(Arg<ApplicationUser> _) => Add(_getRolesSetups, _getRolesCalls, Any<ApplicationUser>());
    public Call<ApplicationUser, IList<Claim>> GetClaimsAsync(ApplicationUser user) => Add(_getClaimsSetups, _getClaimsCalls, actual => user.Equals(actual));
    public Call<ApplicationUser, IList<Claim>> GetClaimsAsync(Arg<ApplicationUser> _) => Add(_getClaimsSetups, _getClaimsCalls, Any<ApplicationUser>());
    public Call<ClaimsPrincipal, ApplicationUser?> GetUserAsync(ClaimsPrincipal principal) => Add(_getUserSetups, _getUserCalls, actual => principal.Equals(actual));
    public Call<ClaimsPrincipal, ApplicationUser?> GetUserAsync(Arg<ClaimsPrincipal> _) => Add(_getUserSetups, _getUserCalls, Any<ClaimsPrincipal>());
    public Call<(ApplicationUser User, string Token), IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token) => Add(_confirmEmailSetups, _confirmEmailCalls, request => user.Equals(request.User) && token.Equals(request.Token, StringComparison.Ordinal));
    public Call<(ApplicationUser User, string Token), IdentityResult> ConfirmEmailAsync(ApplicationUser user, Arg<string> _) => Add(_confirmEmailSetups, _confirmEmailCalls, request => user.Equals(request.User));
    public Call<(ApplicationUser User, string Token), IdentityResult> ConfirmEmailAsync(Arg<ApplicationUser> _, Arg<string> __) => Add(_confirmEmailSetups, _confirmEmailCalls, Any<(ApplicationUser User, string Token)>());
    public Call<ApplicationUser, IdentityResult> RemovePasswordAsync(ApplicationUser user) => Add(_removePasswordSetups, _removePasswordCalls, actual => user.Equals(actual));
    public Call<ApplicationUser, IdentityResult> RemovePasswordAsync(Arg<ApplicationUser> _) => Add(_removePasswordSetups, _removePasswordCalls, Any<ApplicationUser>());
    public Call<(ApplicationUser User, string Password), IdentityResult> AddPasswordAsync(ApplicationUser user, string password) => Add(_addPasswordSetups, _addPasswordCalls, request => user.Equals(request.User) && password.Equals(request.Password, StringComparison.Ordinal));
    public Call<(ApplicationUser User, string Password), IdentityResult> AddPasswordAsync(ApplicationUser user, Arg<string> _) => Add(_addPasswordSetups, _addPasswordCalls, request => user.Equals(request.User));
    public Call<(ApplicationUser User, string Password), IdentityResult> AddPasswordAsync(Arg<ApplicationUser> _, Arg<string> __) => Add(_addPasswordSetups, _addPasswordCalls, Any<(ApplicationUser User, string Password)>());
    public Call<ApplicationUser, string> GeneratePasswordResetTokenAsync(ApplicationUser user) => Add(_generateTokenSetups, _generateTokenCalls, actual => user.Equals(actual));
    public Call<ApplicationUser, string> GeneratePasswordResetTokenAsync(Arg<ApplicationUser> _) => Add(_generateTokenSetups, _generateTokenCalls, Any<ApplicationUser>());
    public Call<(ApplicationUser User, string Token, string Password), IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string password) => Add(_resetPasswordSetups, _resetPasswordCalls, request => user.Equals(request.User) && token.Equals(request.Token, StringComparison.Ordinal) && password.Equals(request.Password, StringComparison.Ordinal));
    public Call<(ApplicationUser User, string Token, string Password), IdentityResult> ResetPasswordAsync(Arg<ApplicationUser> _, Arg<string> __, Arg<string> ___) => Add(_resetPasswordSetups, _resetPasswordCalls, Any<(ApplicationUser User, string Token, string Password)>());

    private static Func<T, bool> Any<T>() => _ => true;

    private static Call<TArgument, TResult> Add<TArgument, TResult>(ICollection<CallSetup<TArgument, TResult>> setups, IReadOnlyList<TArgument> calls, Func<TArgument, bool> matcher)
    {
        var setup = new CallSetup<TArgument, TResult>(matcher);
        setups.Add(setup);
        return new Call<TArgument, TResult>(setup, calls, matcher);
    }

    private static TResult? Resolve<TArgument, TResult>(IReadOnlyList<CallSetup<TArgument, TResult>> setups, ICollection<TArgument> calls, TArgument argument)
    {
        calls.Add(argument);
        for (var index = setups.Count - 1; index >= 0; index--)
        {
            if (setups[index].TryInvoke(argument, out var result))
                return result;
        }

        return default;
    }
}

internal sealed class SignInManagerDouble
{
    private readonly SignInManager<ApplicationUser> _instance;

    public SignInManagerDouble(SignInManager<ApplicationUser> instance) => _instance = instance;

    public SignInManager<ApplicationUser> Instance() => _instance;
}

internal sealed class CallSetup<TArgument, TResult>
{
    private readonly Func<TArgument, bool> _matcher;
    private TResult? _result;

    public CallSetup(Func<TArgument, bool> matcher) => _matcher = matcher;

    public TResult? Result
    {
        get => _result;
        set => _result = value;
    }

    public bool TryInvoke(TArgument argument, out TResult? result)
    {
        if (!_matcher(argument))
        {
            result = default;
            return false;
        }

        result = _result;
        return true;
    }
}

internal sealed class Call<TArgument, TResult>
{
    private readonly CallSetup<TArgument, TResult> _setup;
    private readonly IReadOnlyList<TArgument> _calls;
    private readonly Func<TArgument, bool> _matcher;

    public Call(CallSetup<TArgument, TResult> setup, IReadOnlyList<TArgument> calls, Func<TArgument, bool> matcher)
    {
        _setup = setup;
        _calls = calls;
        _matcher = matcher;
    }

    public Call<TArgument, TResult> ReturnsAsync(TResult value)
    {
        _setup.Result = value;
        return this;
    }

    public void Called(Count expected)
    {
        var actual = _calls.Count(_matcher);
        if (!expected.Matches(actual))
            throw new InvalidOperationException($"Expected {expected} calls but received {actual}.");
    }
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

    public Func<string, ApplicationUser?> FindByName { get; set; } = _ => null;
    public Func<string, ApplicationUser?> FindById { get; set; } = _ => null;
    public Func<ClaimsPrincipal, ApplicationUser?> GetUser { get; set; } = _ => null;
    public Func<ApplicationUser, IList<string>> GetRoles { get; set; } = _ => [];
    public Func<ApplicationUser, IList<Claim>> GetClaims { get; set; } = _ => [];
    public Func<(ApplicationUser User, string Token), IdentityResult> ConfirmEmail { get; set; } = _ => IdentityResult.Success;
    public Func<ApplicationUser, IdentityResult> RemovePassword { get; set; } = _ => IdentityResult.Success;
    public Func<(ApplicationUser User, string Password), IdentityResult> AddPassword { get; set; } = _ => IdentityResult.Success;
    public Func<ApplicationUser, string> GenerateToken { get; set; } = _ => string.Empty;
    public Func<(ApplicationUser User, string Token, string Password), IdentityResult> ResetPassword { get; set; } = _ => IdentityResult.Success;

    public override Task<ApplicationUser?> FindByNameAsync(string userName) => Task.FromResult(FindByName(userName));
    public override Task<ApplicationUser?> FindByIdAsync(string userId) => Task.FromResult(FindById(userId));
    public override Task<IList<string>> GetRolesAsync(ApplicationUser user) => Task.FromResult(GetRoles(user));
    public override Task<IList<Claim>> GetClaimsAsync(ApplicationUser user) => Task.FromResult(GetClaims(user));
    public override Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal) => Task.FromResult(GetUser(principal));
    public override Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token) => Task.FromResult(ConfirmEmail((user, token)));
    public override Task<IdentityResult> RemovePasswordAsync(ApplicationUser user) => Task.FromResult(RemovePassword(user));
    public override Task<IdentityResult> AddPasswordAsync(ApplicationUser user, string password) => Task.FromResult(AddPassword((user, password)));
    public override Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user) => Task.FromResult(GenerateToken(user));
    public override Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword) => Task.FromResult(ResetPassword((user, token, newPassword)));
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
