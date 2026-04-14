using MediatR;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.Factories;
using SimpleResults;

namespace SecurityService.Handlers;

public static class UserInfoHandler
{
    public static async Task<IResult> UserInfoAsync(HttpContext context,
                                                    IMediator mediator,
                                                    CancellationToken cancellationToken) {
        OidcCommands.UserInfoCommand command = new OidcCommands.UserInfoCommand(context);

        Result<UserInfoCommandResult> result = await mediator.Send(command, cancellationToken);

        return OidcResponseFactory.FromResult(result);
    }
}
