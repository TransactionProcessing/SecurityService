using MediatR;
using SecurityService.BusinessLogic.Oidc;
using SimpleResults;

namespace SecurityService.Oidc.Handlers;

public static class UserInfoHandler
{
    public static async Task<IResult> UserInfoAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new OidcCommands.UserInfoCommand(context), cancellationToken);
        return ToIResult(result);
    }

    private static IResult ToIResult(Result<UserInfoCommandResult> result)
    {
        if (result.IsSuccess == false)
        {
            return Results.Problem(result.Message);
        }

        return result.Value switch
        {
            UserInfoJsonResult r      => Results.Json(r.Data),
            UserInfoChallengeResult r => Results.Challenge(r.Properties, r.AuthenticationSchemes),
            _ => Results.StatusCode(500)
        };
    }
}
