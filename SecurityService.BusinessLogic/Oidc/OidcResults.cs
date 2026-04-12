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

// ---- Verify endpoint ----

public abstract record VerifyGetQueryResult;

public sealed record VerifyGetPageResult(string StatusMessage, VerifyDisplayData? Data) : VerifyGetQueryResult;

public sealed record VerifyGetRedirectResult(string Url) : VerifyGetQueryResult;

public abstract record VerifyPostCommandResult;

public sealed record VerifyPostPageResult(string? ModelError, VerifyDisplayData? Data) : VerifyPostCommandResult;

public sealed record VerifyPostRedirectResult(string Url) : VerifyPostCommandResult;

public sealed record VerifyPostForbidResult(string AuthenticationScheme) : VerifyPostCommandResult;

public sealed record VerifyPostSignInResult(ClaimsPrincipal Principal, AuthenticationProperties Properties, string AuthenticationScheme) : VerifyPostCommandResult;

public sealed record VerifyDisplayData(
    string ClientName,
    IReadOnlyCollection<string> RequestedScopes,
    IReadOnlyCollection<ScopeDisplayItem> IdentityScopes,
    IReadOnlyCollection<ScopeDisplayItem> ApiScopes,
    string UserCode);

// ---- Consent endpoint ----

public abstract record ConsentGetQueryResult;

public sealed record ConsentGetPageResult(
    string ClientName,
    IReadOnlyCollection<ScopeDisplayItem> IdentityScopes,
    IReadOnlyCollection<ScopeDisplayItem> ApiScopes) : ConsentGetQueryResult;

public sealed record ConsentGetLocalRedirectResult(string Url) : ConsentGetQueryResult;

public abstract record ConsentPostCommandResult;

public sealed record ConsentPostRedirectResult(string Url) : ConsentPostCommandResult;

public sealed record ConsentPostPageResult(string ModelError) : ConsentPostCommandResult;

// ---- Diagnostics endpoint ----

public abstract record DiagnosticsQueryResult;

public sealed record DiagnosticsPageResult(
    IReadOnlyCollection<DiagnosticItem> Claims,
    IReadOnlyCollection<DiagnosticItem> Properties) : DiagnosticsQueryResult;

public sealed record DiagnosticsNotFoundResult : DiagnosticsQueryResult;

public sealed record DiagnosticsChallengeResult(string AuthenticationScheme) : DiagnosticsQueryResult;

public sealed record DiagnosticItem(string Type, string Value);
