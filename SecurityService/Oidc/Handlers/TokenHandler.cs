using MediatR;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.Factories;

namespace SecurityService.Oidc.Handlers;

public static class TokenHandler
{
    public static async Task<IResult> TokenAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
        => OidcResponseFactory.FromResult(await mediator.Send(new OidcCommands.TokenCommand(context), cancellationToken));
}
