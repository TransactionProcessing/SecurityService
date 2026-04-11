using MediatR;
using Microsoft.AspNetCore.Http;
using SimpleResults;

namespace SecurityService.BusinessLogic.Oidc;

public static class OidcCommands
{
    public sealed record AuthorizeCommand(HttpContext HttpContext) : IRequest<Result<OidcActionResult>>;
    public sealed record TokenCommand(HttpContext HttpContext) : IRequest<Result<OidcActionResult>>;
    public sealed record LogoutCommand(HttpContext HttpContext) : IRequest<Result<OidcActionResult>>;
    public sealed record UserInfoCommand(HttpContext HttpContext) : IRequest<Result<OidcActionResult>>;
}
