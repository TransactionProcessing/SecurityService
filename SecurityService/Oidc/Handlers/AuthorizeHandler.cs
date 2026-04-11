using MediatR;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.Factories;

namespace SecurityService.Oidc.Handlers;

public static class AuthorizeHandler
{
    public static async Task<IResult> AuthorizeAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
        => OidcResponseFactory.FromResult(await mediator.Send(new OidcCommands.AuthorizeCommand(context), cancellationToken));
}
