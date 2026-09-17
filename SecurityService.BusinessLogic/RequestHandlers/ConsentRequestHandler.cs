using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.Database.DbContexts;
using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class ConsentRequestHandler :
    IRequestHandler<OidcCommands.ConsentGetQuery, Result<ConsentGetQueryResult>>,
    IRequestHandler<OidcCommands.ConsentPostCommand, Result<ConsentPostCommandResult>>
{
    private readonly IOpenIddictApplicationManager ApplicationManager;
    private readonly SecurityServiceDbContext DbContext;
    private readonly ConsentTransactionProtector TransactionProtector;

    public ConsentRequestHandler(IOpenIddictApplicationManager applicationManager,
                                 SecurityServiceDbContext dbContext,
                                 ConsentTransactionProtector transactionProtector)
    {
        this.ApplicationManager = applicationManager;
        this.DbContext = dbContext;
        this.TransactionProtector = transactionProtector;
    }

    public async Task<Result<ConsentGetQueryResult>> Handle(OidcCommands.ConsentGetQuery query, CancellationToken cancellationToken)
    {
        var transaction = this.TransactionProtector.Unprotect(query.TransactionId, DateTimeOffset.UtcNow);
        var userId = await GetUserId(query.HttpContext);
        if (transaction is null || userId is null || !string.Equals(transaction.UserId, userId, StringComparison.Ordinal))
        {
            return Result.Success<ConsentGetQueryResult>(new ConsentGetInvalidResult("The consent request is missing, invalid, or expired."));
        }

        var application = await this.ApplicationManager.FindByClientIdAsync(transaction.ClientId, cancellationToken);
        var clientName = application is null
            ? transaction.ClientId
            : await this.ApplicationManager.GetDisplayNameAsync(application, cancellationToken) ?? transaction.ClientId;

        var scopes = await OidcHelpers.BuildScopeDisplay(transaction.RequestedScopes, this.DbContext, cancellationToken);

        return Result.Success<ConsentGetQueryResult>(new ConsentGetPageResult(clientName, scopes.IdentityScopes, scopes.ApiScopes));
    }

    public async Task<Result<ConsentPostCommandResult>> Handle(OidcCommands.ConsentPostCommand command, CancellationToken cancellationToken)
    {
        var transaction = this.TransactionProtector.Unprotect(command.TransactionId, DateTimeOffset.UtcNow);
        var userId = await GetUserId(command.HttpContext);
        if (transaction is null || userId is null || !string.Equals(transaction.UserId, userId, StringComparison.Ordinal))
        {
            return Result.Success<ConsentPostCommandResult>(new ConsentPostInvalidResult("The consent request is missing, invalid, or expired."));
        }

        if (string.Equals(command.Button, "deny", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Success<ConsentPostCommandResult>(
                new ConsentPostRedirectResult(this.BuildDecisionUrl(transaction, "denied", Array.Empty<string>())));
        }

        var grantedScopes = command.SelectedScopes
            .Where(scope => transaction.RequestedScopes.Contains(scope, StringComparer.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (grantedScopes.Length == 0)
        {
            return Result.Success<ConsentPostCommandResult>(
                new ConsentPostPageResult("Select at least one scope to continue."));
        }

        return Result.Success<ConsentPostCommandResult>(new ConsentPostRedirectResult(
            this.BuildDecisionUrl(transaction, "accepted", grantedScopes)));
    }

    private string BuildDecisionUrl(ConsentTransaction transaction, string decision, IEnumerable<string> grantedScopes)
    {
        var redirectUrl = OidcHelpers.AppendQueryValue(transaction.AuthorizationUrl, "consent", decision);
        redirectUrl = OidcHelpers.AppendQueryValue(redirectUrl, "consent_transaction", this.TransactionProtector.Protect(transaction));
        return OidcHelpers.AppendQueryValues(redirectUrl, "granted_scope", grantedScopes);
    }

    private static async Task<string?> GetUserId(HttpContext context)
    {
        if (context.RequestServices is null)
        {
            return null;
        }

        var result = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        return result.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }
}
