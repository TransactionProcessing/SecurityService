using System.Net;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SecurityService.BusinessLogic.Oidc;
using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class DiagnosticsRequestHandler :
    IRequestHandler<OidcCommands.DiagnosticsQuery, Result<DiagnosticsQueryResult>>
{
    public async Task<Result<DiagnosticsQueryResult>> Handle(OidcCommands.DiagnosticsQuery query, CancellationToken cancellationToken)
    {
        var context = query.HttpContext;

        if (context.Connection.RemoteIpAddress is { } remoteIpAddress &&
            IPAddress.IsLoopback(remoteIpAddress) == false)
        {
            return Result.Success<DiagnosticsQueryResult>(new DiagnosticsNotFoundResult());
        }

        var result = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (result.Succeeded == false || result.Principal is null)
        {
            return Result.Success<DiagnosticsQueryResult>(new DiagnosticsChallengeResult(IdentityConstants.ApplicationScheme));
        }

        var claims = result.Principal.Claims
            .Select(claim => new DiagnosticItem(claim.Type, claim.Value))
            .OrderBy(item => item.Type, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var properties = result.Properties?.Items
            .OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .Select(item => new DiagnosticItem(item.Key, item.Value ?? string.Empty))
            .ToArray() ?? Array.Empty<DiagnosticItem>();

        return Result.Success<DiagnosticsQueryResult>(new DiagnosticsPageResult(claims, properties));
    }
}
