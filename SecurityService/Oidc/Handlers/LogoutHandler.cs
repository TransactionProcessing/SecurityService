using MediatR;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.Factories;

namespace SecurityService.Oidc.Handlers;

public static class LogoutHandler
{
    public static async Task<IResult> LogoutAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
        => OidcResponseFactory.FromResult(await mediator.Send(new OidcCommands.LogoutCommand(context), cancellationToken));
}
