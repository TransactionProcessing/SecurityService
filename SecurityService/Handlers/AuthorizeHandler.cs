using MediatR;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.Factories;
using SimpleResults;

namespace SecurityService.Handlers;

public static class AuthorizeHandler
{
    public static async Task<IResult> AuthorizeAsync(HttpContext context,
                                                     IMediator mediator,
                                                     CancellationToken cancellationToken) {
        OidcCommands.AuthorizeCommand command = new OidcCommands.AuthorizeCommand(context);

        Result<AuthorizeCommandResult> result = await mediator.Send(command, cancellationToken);

        return OidcResponseFactory.FromResult(result);
    }
}
