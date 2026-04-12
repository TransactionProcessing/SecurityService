using System.Collections.Immutable;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.Database;
using SecurityService.Database.DbContexts;
using SimpleResults;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class VerifyRequestHandler :
    IRequestHandler<OidcCommands.VerifyGetQuery, Result<VerifyGetQueryResult>>,
    IRequestHandler<OidcCommands.VerifyPostCommand, Result<VerifyPostCommandResult>>
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SecurityServiceDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public VerifyRequestHandler(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        SecurityServiceDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        this._applicationManager = applicationManager;
        this._scopeManager = scopeManager;
        this._dbContext = dbContext;
        this._userManager = userManager;
    }

    public async Task<Result<VerifyGetQueryResult>> Handle(OidcCommands.VerifyGetQuery query, CancellationToken cancellationToken)
    {
        var context = query.HttpContext;
        var userCode = context.Request.Query["user_code"].FirstOrDefault() ?? string.Empty;

        var authenticationResult = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (authenticationResult.Succeeded == false || authenticationResult.Principal?.GetClaim(Claims.ClientId) is null)
        {
            var statusMessage = string.IsNullOrWhiteSpace(userCode) ? string.Empty : "The specified user code is invalid.";
            return Result.Success<VerifyGetQueryResult>(new VerifyGetPageResult(statusMessage, null));
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            var loginUrl = $"/Account/Login?returnUrl={Uri.EscapeDataString(OidcHelpers.BuildCurrentRequestUrl(context.Request))}";
            return Result.Success<VerifyGetQueryResult>(new VerifyGetRedirectResult(loginUrl));
        }

        var data = await this.BuildDisplayDataAsync(authenticationResult, userCode, cancellationToken);
        return Result.Success<VerifyGetQueryResult>(new VerifyGetPageResult(string.Empty, data));
    }

    public async Task<Result<VerifyPostCommandResult>> Handle(OidcCommands.VerifyPostCommand command, CancellationToken cancellationToken)
    {
        var context = command.HttpContext;

        if (string.Equals(command.Action, "lookup", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(command.UserCode))
            {
                return Result.Success<VerifyPostCommandResult>(new VerifyPostPageResult("Enter the user code shown on the device.", null));
            }

            return Result.Success<VerifyPostCommandResult>(new VerifyPostRedirectResult($"/connect/verify?user_code={Uri.EscapeDataString(command.UserCode)}"));
        }

        var authenticationResult = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (authenticationResult.Succeeded == false || authenticationResult.Principal?.GetClaim(Claims.ClientId) is null)
        {
            return Result.Success<VerifyPostCommandResult>(new VerifyPostPageResult("The specified user code is invalid.", null));
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            var loginUrl = $"/Account/Login?returnUrl={Uri.EscapeDataString(OidcHelpers.BuildCurrentRequestUrl(context.Request))}";
            return Result.Success<VerifyPostCommandResult>(new VerifyPostRedirectResult(loginUrl));
        }

        if (string.Equals(command.Action, "deny", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Success<VerifyPostCommandResult>(new VerifyPostForbidResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme));
        }

        var user = await this._userManager.GetUserAsync(context.User);
        if (user is null)
        {
            var loginUrl = $"/Account/Login?returnUrl={Uri.EscapeDataString(OidcHelpers.BuildCurrentRequestUrl(context.Request))}";
            return Result.Success<VerifyPostCommandResult>(new VerifyPostRedirectResult(loginUrl));
        }

        var scopes = authenticationResult.Principal!.GetScopes();
        var resources = await this._scopeManager.ListResourcesAsync(ImmutableArray.CreateRange(scopes), cancellationToken).ToListAsync(cancellationToken);
        var principal = await OidcHelpers.CreatePrincipal(user, this._userManager, scopes, resources, authorizationId: null);

        return Result.Success<VerifyPostCommandResult>(new VerifyPostSignInResult(
            principal,
            new AuthenticationProperties { RedirectUri = "/" },
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme));
    }

    private async Task<VerifyDisplayData> BuildDisplayDataAsync(AuthenticateResult authenticationResult, string userCodeFromQuery, CancellationToken cancellationToken)
    {
        var requestedScopes = authenticationResult.Principal!.GetScopes().ToArray();
        var scopeDisplay = await OidcHelpers.BuildScopeDisplay(requestedScopes, this._dbContext, cancellationToken);

        var userCode = authenticationResult.Properties?.GetTokenValue(OpenIddictServerAspNetCoreConstants.Tokens.UserCode)
            ?? userCodeFromQuery;

        var clientId = authenticationResult.Principal?.GetClaim(Claims.ClientId);
        string clientName;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            clientName = string.Empty;
        }
        else
        {
            var application = await this._applicationManager.FindByClientIdAsync(clientId, cancellationToken);
            clientName = application is null ? clientId : await this._applicationManager.GetDisplayNameAsync(application, cancellationToken) ?? clientId;
        }

        return new VerifyDisplayData(clientName, requestedScopes, scopeDisplay.IdentityScopes, scopeDisplay.ApiScopes, userCode);
    }
}
