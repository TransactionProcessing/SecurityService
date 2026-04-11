using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace SecurityService.BusinessLogic.Oidc;

public abstract record OidcActionResult;

public sealed record OidcSignInResult(ClaimsPrincipal Principal, string AuthenticationScheme) : OidcActionResult;

public sealed record OidcSignOutResult(AuthenticationProperties Properties, IList<string> AuthenticationSchemes) : OidcActionResult;

public sealed record OidcRedirectResult(string Url) : OidcActionResult;

public sealed record OidcForbidResult(AuthenticationProperties Properties, IList<string> AuthenticationSchemes) : OidcActionResult;

public sealed record OidcChallengeResult(AuthenticationProperties Properties, IList<string> AuthenticationSchemes) : OidcActionResult;

public sealed record OidcJsonResult(Dictionary<string, object?> Data) : OidcActionResult;

public sealed record OidcBadRequestResult(object Error) : OidcActionResult;
