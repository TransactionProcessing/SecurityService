using MediatR;
using SecurityService.BusinessLogic.Oidc;
using SimpleResults;

namespace SecurityService.Oidc.Handlers;

public static class AuthorizeHandler
{
    public static async Task<IResult> AuthorizeAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new OidcCommands.AuthorizeCommand(context), cancellationToken);
        return ToIResult(result);
    }

    private static IResult ToIResult(Result<AuthorizeCommandResult> result)
    {
        if (result.IsSuccess == false)
        {
            return Results.Problem(result.Message);
        }

        return result.Value switch
        {
            AuthorizeSignInResult r    => Results.SignIn(r.Principal, properties: null, authenticationScheme: r.AuthenticationScheme),
            AuthorizeRedirectResult r  => Results.Redirect(r.Url),
            AuthorizeForbidResult r    => Results.Forbid(r.Properties, r.AuthenticationSchemes),
            AuthorizeChallengeResult r => Results.Challenge(r.Properties, r.AuthenticationSchemes),
            AuthorizeBadRequestResult r => Results.BadRequest(r.Error),
            _ => Results.StatusCode(500)
        };
    }
}
