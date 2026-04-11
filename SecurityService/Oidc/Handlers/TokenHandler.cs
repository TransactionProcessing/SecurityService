using MediatR;
using SecurityService.BusinessLogic.Oidc;
using SimpleResults;

namespace SecurityService.Oidc.Handlers;

public static class TokenHandler
{
    public static async Task<IResult> TokenAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new OidcCommands.TokenCommand(context), cancellationToken);
        return ToIResult(result);
    }

    private static IResult ToIResult(Result<TokenCommandResult> result)
    {
        if (result.IsSuccess == false)
        {
            return Results.Problem(result.Message);
        }

        return result.Value switch
        {
            TokenSignInResult r     => Results.SignIn(r.Principal, properties: null, authenticationScheme: r.AuthenticationScheme),
            TokenForbidResult r     => Results.Forbid(r.Properties, r.AuthenticationSchemes),
            TokenBadRequestResult r => Results.BadRequest(r.Error),
            _ => Results.StatusCode(500)
        };
    }
}
