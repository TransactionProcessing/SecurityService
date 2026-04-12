using MediatR;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Models;
using SimpleResults;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class GrantRequestHandler :
    IRequestHandler<SecurityServiceQueries.GetUserGrantsQuery, Result<List<GrantDetails>>>,
    IRequestHandler<SecurityServiceCommands.RevokeGrantCommand, Result>
{
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public GrantRequestHandler(IOpenIddictAuthorizationManager authorizationManager, IOpenIddictApplicationManager applicationManager)
    {
        this._authorizationManager = authorizationManager;
        this._applicationManager = applicationManager;
    }

    public async Task<Result<List<GrantDetails>>> Handle(SecurityServiceQueries.GetUserGrantsQuery query, CancellationToken cancellationToken)
    {
        var authorizations = await this._authorizationManager.FindAsync(query.UserId, client: null, status: Statuses.Valid, type: null, scopes: null, cancellationToken).ToListAsync(cancellationToken);
        var grants = new List<GrantDetails>();

        foreach (var authorization in authorizations)
        {
            var grant = await this.BuildGrantDetailsAsync(authorization, cancellationToken);
            if (grant is not null)
            {
                grants.Add(grant);
            }
        }

        var sorted = grants
            .OrderByDescending(grant => grant.CreatedAt)
            .ThenBy(grant => grant.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result.Success(sorted);
    }

    private async Task<GrantDetails?> BuildGrantDetailsAsync(object authorization, CancellationToken cancellationToken)
    {
        var authorizationId = await this._authorizationManager.GetIdAsync(authorization, cancellationToken);
        if (string.IsNullOrWhiteSpace(authorizationId))
        {
            return null;
        }

        var applicationId = await this._authorizationManager.GetApplicationIdAsync(authorization, cancellationToken);
        var (clientId, displayName) = await this.GetApplicationDisplayAsync(applicationId, cancellationToken);

        return new GrantDetails(
            authorizationId,
            clientId,
            displayName,
            await this._authorizationManager.GetScopesAsync(authorization, cancellationToken),
            await this._authorizationManager.GetCreationDateAsync(authorization, cancellationToken));
    }

    private async Task<(string clientId, string displayName)> GetApplicationDisplayAsync(string? applicationId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
        {
            return (string.Empty, string.Empty);
        }

        var application = await this._applicationManager.FindByIdAsync(applicationId, cancellationToken);
        if (application is null)
        {
            return (string.Empty, string.Empty);
        }

        var clientId = await this._applicationManager.GetClientIdAsync(application, cancellationToken) ?? string.Empty;
        var displayName = await this._applicationManager.GetDisplayNameAsync(application, cancellationToken) ?? clientId;
        return (clientId, string.IsNullOrWhiteSpace(displayName) ? clientId : displayName);
    }

    public async Task<Result> Handle(SecurityServiceCommands.RevokeGrantCommand command, CancellationToken cancellationToken)
    {
        var authorization = await this._authorizationManager.FindByIdAsync(command.AuthorizationId, cancellationToken);
        if (authorization is null)
        {
            return Result.NotFound($"No authorization found with id '{command.AuthorizationId}'.");
        }

        var subject = await this._authorizationManager.GetSubjectAsync(authorization, cancellationToken);
        if (string.Equals(subject, command.UserId, StringComparison.Ordinal) == false)
        {
            return Result.NotFound($"No authorization found with id '{command.AuthorizationId}'.");
        }

        return await this._authorizationManager.TryRevokeAsync(authorization, cancellationToken)
            ? Result.Success()
            : Result.Failure("The authorization could not be revoked.");
    }
}
