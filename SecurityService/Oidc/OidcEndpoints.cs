using MediatR;

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

    public static Task<IResult> AuthorizeAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
        => mediator.Send(new OidcCommands.AuthorizeCommand(context), cancellationToken);

    public static Task<IResult> TokenAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
        => mediator.Send(new OidcCommands.TokenCommand(context), cancellationToken);

    public static Task<IResult> LogoutAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
        => mediator.Send(new OidcCommands.LogoutCommand(context), cancellationToken);

    public static Task<IResult> UserInfoAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
        => mediator.Send(new OidcCommands.UserInfoCommand(context), cancellationToken);
}

