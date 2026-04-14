using MediatR;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.Factories;
using SimpleResults;

namespace SecurityService.Handlers;

public static class LogoutHandler
{
    public static async Task<IResult> LogoutAsync(HttpContext context,
                                                  IMediator mediator,
                                                  CancellationToken cancellationToken) {
        OidcCommands.LogoutCommand command = new(context);

        Result<LogoutCommandResult> result = await mediator.Send(command, cancellationToken);

        return OidcResponseFactory.FromResult(result);
    }
}
