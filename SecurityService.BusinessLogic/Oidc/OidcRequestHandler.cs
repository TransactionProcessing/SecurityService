using System.Collections.Immutable;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using SecurityService.Database;
using SecurityService.Database.DbContexts;
using SimpleResults;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.BusinessLogic.Oidc;

public sealed class OidcRequestHandler :
    IRequestHandler<OidcCommands.AuthorizeCommand, Result<AuthorizeCommandResult>>,
    IRequestHandler<OidcCommands.TokenCommand, Result<TokenCommandResult>>,
    IRequestHandler<OidcCommands.LogoutCommand, Result<LogoutCommandResult>>,
    IRequestHandler<OidcCommands.UserInfoCommand, Result<UserInfoCommandResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SecurityServiceDbContext _dbContext;

    public OidcRequestHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        SecurityServiceDbContext dbContext)
    {
        this._userManager = userManager;
        this._signInManager = signInManager;
        this._applicationManager = applicationManager;
        this._authorizationManager = authorizationManager;
        this._scopeManager = scopeManager;
        this._dbContext = dbContext;
    }

    public async Task<Result<AuthorizeCommandResult>> Handle(OidcCommands.AuthorizeCommand command, CancellationToken cancellationToken)
    {
        var context = command.HttpContext;
        var request = context.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        var currentRequestUrl = OidcHelpers.BuildCurrentRequestUrl(context.Request);

        var authenticationResult = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (authenticationResult.Succeeded == false)
        {
            return Result.Success<AuthorizeCommandResult>(HandleAuthenticationFailed(request, currentRequestUrl));
        }

        var user = await this._userManager.GetUserAsync(authenticationResult.Principal);
        if (user is null)
        {
            return Result.Success<AuthorizeCommandResult>(AuthorizeForbidServer(Errors.LoginRequired, "The user account could not be resolved."));
        }

        var application = await this._applicationManager.FindByClientIdAsync(request.ClientId!, cancellationToken);
        if (application is null)
        {
            return Result.Success<AuthorizeCommandResult>(new AuthorizeBadRequestResult(new { error = "invalid_client", error_description = "The client application could not be found." }));
        }

        if (context.Request.Query.TryGetValue("consent", out var consentDecision))
        {
            return Result.Success<AuthorizeCommandResult>(await this.HandleConsentDecision(user, request, application, context, consentDecision!, cancellationToken));
        }

        return Result.Success<AuthorizeCommandResult>(await this.HandleConsentType(user, request, application, currentRequestUrl, cancellationToken));
    }

    public async Task<Result<TokenCommandResult>> Handle(OidcCommands.TokenCommand command, CancellationToken cancellationToken)
    {
        var context = command.HttpContext;
        var request = context.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType() || request.IsDeviceCodeGrantType())
        {
            return Result.Success<TokenCommandResult>(await this.HandleCodeOrRefreshToken(context, cancellationToken));
        }

        if (request.IsClientCredentialsGrantType())
        {
            return Result.Success<TokenCommandResult>(await this.HandleClientCredentials(request, cancellationToken));
        }

        if (request.IsPasswordGrantType())
        {
            return Result.Success<TokenCommandResult>(await this.HandlePasswordGrant(request, cancellationToken));
        }

        return Result.Success<TokenCommandResult>(new TokenBadRequestResult(new { error = "unsupported_grant_type", error_description = "The specified grant type is not supported by this service." }));
    }

    public Task<Result<LogoutCommandResult>> Handle(OidcCommands.LogoutCommand command, CancellationToken cancellationToken)
    {
        var context = command.HttpContext;
        var currentRequestUrl = OidcHelpers.BuildCurrentRequestUrl(context.Request);
        var confirmed = string.Equals(context.Request.Query["logout"], "confirmed", StringComparison.OrdinalIgnoreCase);
        if (confirmed)
        {
            LogoutCommandResult signOut = new LogoutSignOutResult(
                new AuthenticationProperties { RedirectUri = "/Account/Logout/LoggedOut" },
                [IdentityConstants.ApplicationScheme, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
            return Task.FromResult(Result.Success(signOut));
        }

        LogoutCommandResult redirect = new LogoutRedirectResult($"/Account/Logout?returnUrl={Uri.EscapeDataString(currentRequestUrl)}");
        return Task.FromResult(Result.Success(redirect));
    }

    public async Task<Result<UserInfoCommandResult>> Handle(OidcCommands.UserInfoCommand command, CancellationToken cancellationToken)
    {
        var context = command.HttpContext;
        var authenticationResult = await context.AuthenticateAsync(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        if (authenticationResult.Succeeded == false || authenticationResult.Principal is null)
        {
            return Result.Success<UserInfoCommandResult>(InvalidToken("The access token is missing or invalid."));
        }

        var subject = authenticationResult.Principal.GetClaim(Claims.Subject);
        if (string.IsNullOrWhiteSpace(subject))
        {
            return Result.Success<UserInfoCommandResult>(InvalidToken("The access token does not contain a subject identifier."));
        }

        var user = await this._userManager.FindByIdAsync(subject);
        if (user is null)
        {
            return Result.Success<UserInfoCommandResult>(InvalidToken("The user associated with the token could not be found."));
        }

        var scopes = authenticationResult.Principal.GetScopes().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var response = await this.BuildUserInfoResponse(user, scopes);
        return Result.Success<UserInfoCommandResult>(new UserInfoJsonResult(response.Where(pair => pair.Value is not null).ToDictionary(pair => pair.Key, pair => pair.Value)));
    }

    private static AuthorizeCommandResult HandleAuthenticationFailed(OpenIddictRequest request, string currentRequestUrl)
    {
        if (request.HasPromptValue(PromptValues.None))
        {
            return AuthorizeForbidServer(Errors.LoginRequired, "The user is not logged in.");
        }

        return new AuthorizeChallengeResult(new AuthenticationProperties { RedirectUri = currentRequestUrl }, [IdentityConstants.ApplicationScheme]);
    }

    private async Task<AuthorizeCommandResult> HandleConsentDecision(
        ApplicationUser user,
        OpenIddictRequest request,
        object application,
        HttpContext context,
        string consentDecision,
        CancellationToken cancellationToken)
    {
        if (string.Equals(consentDecision, "denied", StringComparison.OrdinalIgnoreCase))
        {
            return AuthorizeForbidServer(Errors.AccessDenied, "The authorization request was denied.");
        }

        if (string.Equals(consentDecision, "accepted", StringComparison.OrdinalIgnoreCase))
        {
            return await this.CompleteAuthorization(user, request, application, cancellationToken, OidcHelpers.ReadMultiValue(context.Request.Query, "granted_scope"));
        }

        return await this.HandleConsentType(user, request, application, OidcHelpers.BuildCurrentRequestUrl(context.Request), cancellationToken);
    }

    private async Task<AuthorizeCommandResult> HandleConsentType(
        ApplicationUser user,
        OpenIddictRequest request,
        object application,
        string currentRequestUrl,
        CancellationToken cancellationToken)
    {
        var consentType = await this._applicationManager.GetConsentTypeAsync(application, cancellationToken) ?? ConsentTypes.Implicit;
        var existingAuthorizations = await this._authorizationManager.FindAsync(
            subject: user.Id,
            client: await this._applicationManager.GetIdAsync(application, cancellationToken),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: ImmutableArray.CreateRange(request.GetScopes()),
            cancellationToken: cancellationToken).ToListAsync(cancellationToken);

        if (string.Equals(consentType, ConsentTypes.Explicit, StringComparison.OrdinalIgnoreCase) && existingAuthorizations.Count == 0)
        {
            if (request.HasPromptValue(PromptValues.None))
            {
                return AuthorizeForbidServer(Errors.ConsentRequired, "Interactive user consent is required.");
            }

            return new AuthorizeRedirectResult($"/Consent?returnUrl={Uri.EscapeDataString(currentRequestUrl)}");
        }

        return await this.CompleteAuthorization(user, request, application, cancellationToken, request.GetScopes());
    }

    private async Task<TokenCommandResult> HandleCodeOrRefreshToken(HttpContext context, CancellationToken cancellationToken)
    {
        var authenticationResult = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (authenticationResult.Succeeded == false || authenticationResult.Principal is null)
        {
            return InvalidGrant("The specified token is invalid.");
        }

        var subject = authenticationResult.Principal.GetClaim(Claims.Subject);
        if (string.IsNullOrWhiteSpace(subject))
        {
            return new TokenSignInResult(authenticationResult.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var user = await this._userManager.FindByIdAsync(subject);
        if (user is null || await this._signInManager.CanSignInAsync(user) == false)
        {
            return InvalidGrant("The token is no longer valid.");
        }

        var scopes = authenticationResult.Principal.GetScopes();
        var resources = await this._scopeManager.ListResourcesAsync(ImmutableArray.CreateRange(scopes), cancellationToken).ToListAsync(cancellationToken);
        var principal = await OidcHelpers.CreatePrincipal(user, this._userManager, scopes, resources, authenticationResult.Principal.GetAuthorizationId());
        return new TokenSignInResult(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<TokenCommandResult> HandleClientCredentials(OpenIddictRequest request, CancellationToken cancellationToken)
    {
        var grantedScopes = await OidcHelpers.ResolveClientCredentialsScopes(request, this._dbContext, cancellationToken);
        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);
        identity.SetClaim(Claims.Subject, request.ClientId!)
                .SetClaim(Claims.Name, request.ClientId!);
        identity.SetScopes(grantedScopes);
        identity.SetResources(await this._scopeManager.ListResourcesAsync(ImmutableArray.CreateRange(grantedScopes), cancellationToken).ToListAsync(cancellationToken));
        identity.SetDestinations(OidcHelpers.GetDestinations);
        return new TokenSignInResult(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<TokenCommandResult> HandlePasswordGrant(OpenIddictRequest request, CancellationToken cancellationToken)
    {
        var user = await this._userManager.FindByNameAsync(request.Username!);
        if (user is null)
        {
            return InvalidGrant();
        }

        var signInResult = await this._signInManager.CheckPasswordSignInAsync(user, request.Password!, lockoutOnFailure: true);
        if (signInResult.Succeeded == false)
        {
            return InvalidGrant();
        }

        var grantedScopes = await OidcHelpers.ResolveClientCredentialsScopes(request, this._dbContext, cancellationToken);
        var resources = await this._scopeManager.ListResourcesAsync(ImmutableArray.CreateRange(grantedScopes), cancellationToken).ToListAsync(cancellationToken);
        var principal = await OidcHelpers.CreatePrincipal(user, this._userManager, grantedScopes, resources, authorizationId: null);
        return new TokenSignInResult(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<Dictionary<string, object?>> BuildUserInfoResponse(ApplicationUser user, HashSet<string> scopes)
    {
        var response = new Dictionary<string, object?> { [Claims.Subject] = user.Id };

        if (scopes.Contains(Scopes.Profile))
        {
            response[Claims.Name] = user.UserName;
            response[Claims.PreferredUsername] = user.UserName;
            response[Claims.GivenName] = user.GivenName;
            response[Claims.FamilyName] = user.FamilyName;
            response["middle_name"] = user.MiddleName;
        }

        if (scopes.Contains(Scopes.Email))
        {
            response[Claims.Email] = user.Email;
            response[Claims.EmailVerified] = user.EmailConfirmed;
        }

        if (scopes.Contains(Scopes.Roles))
        {
            response[Claims.Role] = (await this._userManager.GetRolesAsync(user)).ToArray();
        }

        foreach (var claim in await this._userManager.GetClaimsAsync(user))
        {
            if (response.ContainsKey(claim.Type) == false)
            {
                response[claim.Type] = claim.Value;
            }
        }

        return response;
    }

    private static AuthorizeForbidResult AuthorizeForbidServer(string error, string description) =>
        new AuthorizeForbidResult(
            new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            }),
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);

    private async Task<AuthorizeCommandResult> CompleteAuthorization(
        ApplicationUser user,
        OpenIddictRequest request,
        object application,
        CancellationToken cancellationToken,
        IReadOnlyCollection<string> grantedScopes)
    {
        var resources = await this._scopeManager.ListResourcesAsync(ImmutableArray.CreateRange(grantedScopes), cancellationToken).ToListAsync(cancellationToken);
        var authorizations = await this._authorizationManager.FindAsync(
            subject: user.Id,
            client: await this._applicationManager.GetIdAsync(application, cancellationToken),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: ImmutableArray.CreateRange(grantedScopes),
            cancellationToken: cancellationToken).ToListAsync(cancellationToken);

        var authorization = authorizations.LastOrDefault();
        var principal = await OidcHelpers.CreatePrincipal(user, this._userManager, grantedScopes, resources, authorizationId: null);

        authorization ??= await this._authorizationManager.CreateAsync(
            principal: principal,
            subject: user.Id,
            client: (await this._applicationManager.GetIdAsync(application, cancellationToken))!,
            type: AuthorizationTypes.Permanent,
            scopes: ImmutableArray.CreateRange(grantedScopes),
            cancellationToken: cancellationToken);

        principal.SetAuthorizationId(await this._authorizationManager.GetIdAsync(authorization, cancellationToken));
        return new AuthorizeSignInResult(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static TokenForbidResult InvalidGrant(string description = "The username/password couple is invalid.") =>
        new TokenForbidResult(
            new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            }),
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);

    private static UserInfoChallengeResult InvalidToken(string description) =>
        new UserInfoChallengeResult(
            new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictValidationAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                [OpenIddictValidationAspNetCoreConstants.Properties.ErrorDescription] = description
            }),
            [OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme]);
}
