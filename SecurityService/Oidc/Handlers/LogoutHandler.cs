using MediatR;
using SecurityService.BusinessLogic.Oidc;
using SimpleResults;

namespace SecurityService.Oidc.Handlers;

public static class LogoutHandler
{
    public static async Task<IResult> LogoutAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new OidcCommands.LogoutCommand(context), cancellationToken);
        return ToIResult(result);
    }

    private static IResult ToIResult(Result<LogoutCommandResult> result)
    {
        if (result.IsSuccess == false)
        {
            return Results.Problem(result.Message);
        }

        return result.Value switch
        {
            LogoutSignOutResult r  => Results.SignOut(r.Properties, r.AuthenticationSchemes),
            LogoutRedirectResult r => Results.Redirect(r.Url),
            _ => Results.StatusCode(500)
        };
    }
}
