using MediatR;
using Microsoft.AspNetCore.Http;
using SimpleResults;

namespace SecurityService.BusinessLogic.Oidc;

public static class OidcCommands
{
    public sealed record AuthorizeCommand(HttpContext HttpContext) : IRequest<Result<AuthorizeCommandResult>>;
    public sealed record TokenCommand(HttpContext HttpContext) : IRequest<Result<TokenCommandResult>>;
    public sealed record LogoutCommand(HttpContext HttpContext) : IRequest<Result<LogoutCommandResult>>;
    public sealed record UserInfoCommand(HttpContext HttpContext) : IRequest<Result<UserInfoCommandResult>>;
}
