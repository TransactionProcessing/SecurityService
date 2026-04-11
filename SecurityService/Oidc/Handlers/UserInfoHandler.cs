using MediatR;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.Factories;

namespace SecurityService.Oidc.Handlers;

public static class UserInfoHandler
{
    public static async Task<IResult> UserInfoAsync(HttpContext context, IMediator mediator, CancellationToken cancellationToken)
        => OidcResponseFactory.FromResult(await mediator.Send(new OidcCommands.UserInfoCommand(context), cancellationToken));
}
