using MediatR;
using Microsoft.AspNetCore.Authentication;
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

    public static IResult ToIResult(Result<OidcActionResult> result)
    {
        if (result.IsSuccess == false)
        {
            return Results.Problem(result.Message);
        }

        return ToIResult(result.Value!);
    }

    public static IResult ToIResult(OidcActionResult action) => action switch
    {
        OidcSignInResult r => Results.SignIn(r.Principal, properties: null, authenticationScheme: r.AuthenticationScheme),
        OidcSignOutResult r => Results.SignOut(r.Properties, r.AuthenticationSchemes),
        OidcRedirectResult r => Results.Redirect(r.Url),
        OidcForbidResult r => Results.Forbid(r.Properties, r.AuthenticationSchemes),
        OidcChallengeResult r => Results.Challenge(r.Properties, r.AuthenticationSchemes),
        OidcJsonResult r => Results.Json(r.Data),
        OidcBadRequestResult r => Results.BadRequest(r.Error),
        _ => Results.StatusCode(500)
    };
}
