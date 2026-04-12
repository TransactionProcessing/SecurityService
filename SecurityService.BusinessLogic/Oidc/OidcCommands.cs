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
    public sealed record VerifyGetQuery(HttpContext HttpContext) : IRequest<Result<VerifyGetQueryResult>>;
    public sealed record VerifyPostCommand(HttpContext HttpContext, string Action, string UserCode) : IRequest<Result<VerifyPostCommandResult>>;
    public sealed record ConsentGetQuery(HttpContext HttpContext, string ReturnUrl) : IRequest<Result<ConsentGetQueryResult>>;
    public sealed record ConsentPostCommand(string ReturnUrl, string Button, IReadOnlyCollection<string> SelectedScopes) : IRequest<Result<ConsentPostCommandResult>>;
}
