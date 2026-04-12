using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace SecurityService.BusinessLogic.Oidc;

// ---- Authorize endpoint ----

public abstract record AuthorizeCommandResult;

public sealed record AuthorizeSignInResult(ClaimsPrincipal Principal, string AuthenticationScheme) : AuthorizeCommandResult;

public sealed record AuthorizeRedirectResult(string Url) : AuthorizeCommandResult;

public sealed record AuthorizeForbidResult(AuthenticationProperties Properties, IList<string> AuthenticationSchemes) : AuthorizeCommandResult;

public sealed record AuthorizeChallengeResult(AuthenticationProperties Properties, IList<string> AuthenticationSchemes) : AuthorizeCommandResult;

public sealed record AuthorizeBadRequestResult(object Error) : AuthorizeCommandResult;

// ---- Token endpoint ----

public abstract record TokenCommandResult;

public sealed record TokenSignInResult(ClaimsPrincipal Principal, string AuthenticationScheme) : TokenCommandResult;

public sealed record TokenForbidResult(AuthenticationProperties Properties, IList<string> AuthenticationSchemes) : TokenCommandResult;

public sealed record TokenBadRequestResult(object Error) : TokenCommandResult;

// ---- Logout endpoint ----

public abstract record LogoutCommandResult;

public sealed record LogoutSignOutResult(AuthenticationProperties Properties, IList<string> AuthenticationSchemes) : LogoutCommandResult;

public sealed record LogoutRedirectResult(string Url) : LogoutCommandResult;

// ---- UserInfo endpoint ----

public abstract record UserInfoCommandResult;

public sealed record UserInfoJsonResult(Dictionary<string, object?> Data) : UserInfoCommandResult;

public sealed record UserInfoChallengeResult(AuthenticationProperties Properties, IList<string> AuthenticationSchemes) : UserInfoCommandResult;
