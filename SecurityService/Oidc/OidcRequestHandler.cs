using System.Collections.Immutable;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using SecurityService.Database;
using SecurityService.Database.DbContexts;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.Oidc;

public sealed class OidcRequestHandler :
    IRequestHandler<OidcCommands.AuthorizeCommand, IResult>,
    IRequestHandler<OidcCommands.TokenCommand, IResult>,
    IRequestHandler<OidcCommands.LogoutCommand, IResult>,
    IRequestHandler<OidcCommands.UserInfoCommand, IResult>
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

    public async Task<IResult> Handle(OidcCommands.AuthorizeCommand command, CancellationToken cancellationToken)
    {
        var context = command.HttpContext;
        var request = context.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        var currentRequestUrl = OidcHelpers.BuildCurrentRequestUrl(context.Request);

        var authenticationResult = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (authenticationResult.Succeeded == false)
        {
            if (request.HasPromptValue(PromptValues.None))
            {
                return Results.Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                }), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
            }

            return Results.Challenge(new AuthenticationProperties { RedirectUri = currentRequestUrl }, [IdentityConstants.ApplicationScheme]);
        }

        var user = await this._userManager.GetUserAsync(authenticationResult.Principal);
        if (user is null)
        {
            return Results.Forbid(new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user account could not be resolved."
            }), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

        var application = await this._applicationManager.FindByClientIdAsync(request.ClientId!, cancellationToken);
        if (application is null)
        {
            return Results.BadRequest(new { error = "invalid_client", error_description = "The client application could not be found." });
        }

        if (context.Request.Query.TryGetValue("consent", out var consentDecision))
        {
            if (string.Equals(consentDecision, "denied", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.AccessDenied,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The authorization request was denied."
                }), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
            }

            if (string.Equals(consentDecision, "accepted", StringComparison.OrdinalIgnoreCase))
            {
                return await this.CompleteAuthorizationAsync(user, request, application, cancellationToken, OidcHelpers.ReadMultiValue(context.Request.Query, "granted_scope"));
            }
        }

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
                return Results.Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Interactive user consent is required."
                }), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
            }

            return Results.Redirect($"/Consent?returnUrl={Uri.EscapeDataString(currentRequestUrl)}");
        }

        return await this.CompleteAuthorizationAsync(user, request, application, cancellationToken, request.GetScopes());
    }

    public async Task<IResult> Handle(OidcCommands.TokenCommand command, CancellationToken cancellationToken)
    {
        var context = command.HttpContext;
        var request = context.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType() || request.IsDeviceCodeGrantType())
        {
            var authenticationResult = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (authenticationResult.Succeeded == false || authenticationResult.Principal is null)
            {
                return InvalidGrant("The specified token is invalid.");
            }

            var subject = authenticationResult.Principal.GetClaim(Claims.Subject);
            if (string.IsNullOrWhiteSpace(subject))
            {
                return Results.SignIn(authenticationResult.Principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var user = await this._userManager.FindByIdAsync(subject);
            if (user is null || await this._signInManager.CanSignInAsync(user) == false)
            {
                return InvalidGrant("The token is no longer valid.");
            }

            var principal = await OidcHelpers.CreatePrincipalAsync(
                user,
                this._userManager,
                authenticationResult.Principal.GetScopes(),
                await this._scopeManager.ListResourcesAsync(ImmutableArray.CreateRange(authenticationResult.Principal.GetScopes()), cancellationToken).ToListAsync(cancellationToken),
                authenticationResult.Principal.GetAuthorizationId());
            return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsClientCredentialsGrantType())
        {
            var grantedScopes = await OidcHelpers.ResolveClientCredentialsScopesAsync(request, this._dbContext, cancellationToken);

            var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);
            identity.SetClaim(Claims.Subject, request.ClientId!)
                    .SetClaim(Claims.Name, request.ClientId!);
            identity.SetScopes(grantedScopes);
            identity.SetResources(await this._scopeManager.ListResourcesAsync(ImmutableArray.CreateRange(grantedScopes), cancellationToken).ToListAsync(cancellationToken));
            identity.SetDestinations(OidcHelpers.GetDestinations);
            return Results.SignIn(new ClaimsPrincipal(identity), authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsPasswordGrantType())
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

            var grantedScopes = await OidcHelpers.ResolveClientCredentialsScopesAsync(request, this._dbContext, cancellationToken);

            var resources = await this._scopeManager.ListResourcesAsync(ImmutableArray.CreateRange(grantedScopes), cancellationToken).ToListAsync(cancellationToken);
            var principal = await OidcHelpers.CreatePrincipalAsync(user, this._userManager, grantedScopes, resources, authorizationId: null);
            return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return Results.BadRequest(new { error = "unsupported_grant_type", error_description = "The specified grant type is not supported by this service." });
    }

    public Task<IResult> Handle(OidcCommands.LogoutCommand command, CancellationToken cancellationToken)
    {
        var context = command.HttpContext;
        var currentRequestUrl = OidcHelpers.BuildCurrentRequestUrl(context.Request);
        var confirmed = string.Equals(context.Request.Query["logout"], "confirmed", StringComparison.OrdinalIgnoreCase);
        if (confirmed)
        {
            return Task.FromResult<IResult>(Results.SignOut(new AuthenticationProperties { RedirectUri = "/Account/Logout/LoggedOut" }, [IdentityConstants.ApplicationScheme, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]));
        }

        return Task.FromResult<IResult>(Results.Redirect($"/Account/Logout?returnUrl={Uri.EscapeDataString(currentRequestUrl)}"));
    }

    public async Task<IResult> Handle(OidcCommands.UserInfoCommand command, CancellationToken cancellationToken)
    {
        var context = command.HttpContext;
        var authenticationResult = await context.AuthenticateAsync(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        if (authenticationResult.Succeeded == false || authenticationResult.Principal is null)
        {
            return InvalidToken("The access token is missing or invalid.");
        }

        var subject = authenticationResult.Principal.GetClaim(Claims.Subject);
        if (string.IsNullOrWhiteSpace(subject))
        {
            return InvalidToken("The access token does not contain a subject identifier.");
        }

        var user = await this._userManager.FindByIdAsync(subject);
        if (user is null)
        {
            return InvalidToken("The user associated with the token could not be found.");
        }

        var scopes = authenticationResult.Principal.GetScopes().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var response = new Dictionary<string, object?>
        {
            [Claims.Subject] = user.Id
        };

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

        return Results.Json(response.Where(pair => pair.Value is not null).ToDictionary(pair => pair.Key, pair => pair.Value));
    }

    private async Task<IResult> CompleteAuthorizationAsync(
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
        var principal = await OidcHelpers.CreatePrincipalAsync(user, this._userManager, grantedScopes, resources, authorizationId: null);

        authorization ??= await this._authorizationManager.CreateAsync(
            principal: principal,
            subject: user.Id,
            client: (await this._applicationManager.GetIdAsync(application, cancellationToken))!,
            type: AuthorizationTypes.Permanent,
            scopes: ImmutableArray.CreateRange(grantedScopes),
            cancellationToken: cancellationToken);

        principal.SetAuthorizationId(await this._authorizationManager.GetIdAsync(authorization, cancellationToken));
        return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static IResult InvalidGrant(string description = "The username/password couple is invalid.") =>
        Results.Forbid(new AuthenticationProperties(new Dictionary<string, string?>
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
        }), [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);

    private static IResult InvalidToken(string description) =>
        Results.Challenge(new AuthenticationProperties(new Dictionary<string, string?>
        {
            [OpenIddictValidationAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
            [OpenIddictValidationAspNetCoreConstants.Properties.ErrorDescription] = description
        }), [OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme]);
}
