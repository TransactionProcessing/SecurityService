using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
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

    public ConsentRequestHandler(IOpenIddictApplicationManager applicationManager,
                                 SecurityServiceDbContext dbContext)
    {
        this.ApplicationManager = applicationManager;
        this.DbContext = dbContext;
    }

    public async Task<Result<ConsentGetQueryResult>> Handle(OidcCommands.ConsentGetQuery query, CancellationToken cancellationToken)
    {
        var request = query.HttpContext.GetOpenIddictServerRequest();
        if (request is null)
        {
            return Result.Success<ConsentGetQueryResult>(new ConsentGetLocalRedirectResult(query.ReturnUrl));
        }

        var application = await this.ApplicationManager.FindByClientIdAsync(request.ClientId!, cancellationToken);
        var clientName = application is null
            ? request.ClientId!
            : await this.ApplicationManager.GetDisplayNameAsync(application, cancellationToken) ?? request.ClientId!;

        var scopes = await OidcHelpers.BuildScopeDisplay(request, this.DbContext, cancellationToken);

        return Result.Success<ConsentGetQueryResult>(new ConsentGetPageResult(clientName, scopes.IdentityScopes, scopes.ApiScopes));
    }

    public Task<Result<ConsentPostCommandResult>> Handle(OidcCommands.ConsentPostCommand command, CancellationToken cancellationToken)
    {
        if (string.Equals(command.Button, "deny", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(Result.Success<ConsentPostCommandResult>(
                new ConsentPostRedirectResult(OidcHelpers.AppendQueryValue(command.ReturnUrl, "consent", "denied"))));
        }

        if (command.SelectedScopes.Count == 0)
        {
            return Task.FromResult(Result.Success<ConsentPostCommandResult>(
                new ConsentPostPageResult("Select at least one scope to continue.")));
        }

        var redirectUrl = OidcHelpers.AppendQueryValue(command.ReturnUrl, "consent", "accepted");
        redirectUrl = OidcHelpers.AppendQueryValues(redirectUrl, "granted_scope", command.SelectedScopes);
        return Task.FromResult(Result.Success<ConsentPostCommandResult>(new ConsentPostRedirectResult(redirectUrl)));
    }
}
