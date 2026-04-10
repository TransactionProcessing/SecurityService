using MediatR;
using Microsoft.AspNetCore.Http;

namespace SecurityService.Oidc;

public static class OidcCommands
{
    public sealed record AuthorizeCommand(HttpContext HttpContext) : IRequest<IResult>;
    public sealed record TokenCommand(HttpContext HttpContext) : IRequest<IResult>;
    public sealed record LogoutCommand(HttpContext HttpContext) : IRequest<IResult>;
    public sealed record UserInfoCommand(HttpContext HttpContext) : IRequest<IResult>;
}
