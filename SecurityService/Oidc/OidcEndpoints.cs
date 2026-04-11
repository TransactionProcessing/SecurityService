using MediatR;
using SecurityService.BusinessLogic.Oidc;
using SimpleResults;

namespace SecurityService.Oidc;

public static class OidcEndpoints
{
    public static void MapOidcEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods("/connect/authorize", [HttpMethods.Get, HttpMethods.Post], (Delegate)AuthorizeAsync);
        endpoints.MapPost("/connect/token", (Delegate)TokenAsync);
        endpoints.MapMethods("/connect/logout", [HttpMethods.Get, HttpMethods.Post], (Delegate)LogoutAsync);
        endpoints.MapMethods("/connect/userinfo", [HttpMethods.Get, HttpMethods.Post], (Delegate)UserInfoAsync);
    }

    public static async Task<IResult> AuthorizeAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new OidcCommands.AuthorizeCommand(context), cancellationToken);
        return ToIResult(result);
    }

    public static async Task<IResult> TokenAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new OidcCommands.TokenCommand(context), cancellationToken);
        return ToIResult(result);
    }

    public static async Task<IResult> LogoutAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new OidcCommands.LogoutCommand(context), cancellationToken);
        return ToIResult(result);
    }

    public static async Task<IResult> UserInfoAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new OidcCommands.UserInfoCommand(context), cancellationToken);
        return ToIResult(result);
    }

    public static IResult ToIResult(Result<AuthorizeCommandResult> result)
    {
        if (result.IsSuccess == false)
        {
            return Results.Problem(result.Message);
        }

        return result.Value switch
        {
            AuthorizeSignInResult r  => Results.SignIn(r.Principal, properties: null, authenticationScheme: r.AuthenticationScheme),
            AuthorizeRedirectResult r => Results.Redirect(r.Url),
            AuthorizeForbidResult r  => Results.Forbid(r.Properties, r.AuthenticationSchemes),
            AuthorizeChallengeResult r => Results.Challenge(r.Properties, r.AuthenticationSchemes),
            AuthorizeBadRequestResult r => Results.BadRequest(r.Error),
            _ => Results.StatusCode(500)
        };
    }

    public static IResult ToIResult(Result<TokenCommandResult> result)
    {
        if (result.IsSuccess == false)
        {
            return Results.Problem(result.Message);
        }

        return result.Value switch
        {
            TokenSignInResult r  => Results.SignIn(r.Principal, properties: null, authenticationScheme: r.AuthenticationScheme),
            TokenForbidResult r  => Results.Forbid(r.Properties, r.AuthenticationSchemes),
            TokenBadRequestResult r => Results.BadRequest(r.Error),
            _ => Results.StatusCode(500)
        };
    }

    public static IResult ToIResult(Result<LogoutCommandResult> result)
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

    public static IResult ToIResult(Result<UserInfoCommandResult> result)
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
