using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using SecurityService.Models;
using SimpleResults;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityService.Services;

public interface IGrantService
{
    Task<IReadOnlyCollection<GrantDetails>> GetUserGrantsAsync(string userId, CancellationToken cancellationToken);

    Task<Result> RevokeAsync(string userId, string authorizationId, CancellationToken cancellationToken);
}

public sealed class GrantService : IGrantService
{
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public GrantService(IOpenIddictAuthorizationManager authorizationManager, IOpenIddictApplicationManager applicationManager)
    {
        this._authorizationManager = authorizationManager;
        this._applicationManager = applicationManager;
    }

    public async Task<IReadOnlyCollection<GrantDetails>> GetUserGrantsAsync(string userId, CancellationToken cancellationToken)
    {
        var authorizations = await this._authorizationManager.FindAsync(userId, client: null, status: Statuses.Valid, type: null, scopes: null, cancellationToken).ToListAsync(cancellationToken);
        var grants = new List<GrantDetails>();

        foreach (var authorization in authorizations)
        {
            var authorizationId = await this._authorizationManager.GetIdAsync(authorization, cancellationToken);
            if (string.IsNullOrWhiteSpace(authorizationId))
            {
                continue;
            }

            var applicationId = await this._authorizationManager.GetApplicationIdAsync(authorization, cancellationToken);
            object? application = string.IsNullOrWhiteSpace(applicationId) ? null : await this._applicationManager.FindByIdAsync(applicationId, cancellationToken);
            var clientId = application is null ? string.Empty : await this._applicationManager.GetClientIdAsync(application, cancellationToken) ?? string.Empty;
            var displayName = application is null ? clientId : await this._applicationManager.GetDisplayNameAsync(application, cancellationToken) ?? clientId;

            grants.Add(new GrantDetails(
                authorizationId,
                clientId,
                string.IsNullOrWhiteSpace(displayName) ? clientId : displayName,
                await this._authorizationManager.GetScopesAsync(authorization, cancellationToken),
                await this._authorizationManager.GetCreationDateAsync(authorization, cancellationToken)));
        }

        return grants
            .OrderByDescending(grant => grant.CreatedAt)
            .ThenBy(grant => grant.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<Result> RevokeAsync(string userId, string authorizationId, CancellationToken cancellationToken)
    {
        var authorization = await this._authorizationManager.FindByIdAsync(authorizationId, cancellationToken);
        if (authorization is null)
        {
            return Result.NotFound($"No authorization found with id '{authorizationId}'.");
        }

        var subject = await this._authorizationManager.GetSubjectAsync(authorization, cancellationToken);
        if (string.Equals(subject, userId, StringComparison.Ordinal) == false)
        {
            return Result.NotFound($"No authorization found with id '{authorizationId}'.");
        }

        return await this._authorizationManager.TryRevokeAsync(authorization, cancellationToken)
            ? Result.Success()
            : Result.Failure("The authorization could not be revoked.");
    }
}


