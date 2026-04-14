using MediatR;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.Factories;
using SimpleResults;

namespace SecurityService.Handlers;

public static class TokenHandler
{
    public static async Task<IResult> TokenAsync(HttpContext context,
                                                 IMediator mediator,
                                                 CancellationToken cancellationToken) {
        OidcCommands.TokenCommand command = new(context);
        
        Result<TokenCommandResult> result = await mediator.Send(command, cancellationToken);

        return OidcResponseFactory.FromResult(result);
    }
}
