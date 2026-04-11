using SecurityService.BusinessLogic.Oidc;
using SimpleResults;

namespace SecurityService.Factories;

public static class OidcResponseFactory
{
    public static IResult FromResult(Result<AuthorizeCommandResult> result)
    {
        if (result.IsSuccess == false)
        {
            return Results.Problem(result.Message);
        }

        return result.Value switch
        {
            AuthorizeSignInResult r     => Results.SignIn(r.Principal, properties: null, authenticationScheme: r.AuthenticationScheme),
            AuthorizeRedirectResult r   => Results.Redirect(r.Url),
            AuthorizeForbidResult r     => Results.Forbid(r.Properties, r.AuthenticationSchemes),
            AuthorizeChallengeResult r  => Results.Challenge(r.Properties, r.AuthenticationSchemes),
            AuthorizeBadRequestResult r => Results.BadRequest(r.Error),
            _ => Results.StatusCode(500)
        };
    }

    public static IResult FromResult(Result<TokenCommandResult> result)
    {
        if (result.IsSuccess == false)
        {
            return Results.Problem(result.Message);
        }

        return result.Value switch
        {
            TokenSignInResult r     => Results.SignIn(r.Principal, properties: null, authenticationScheme: r.AuthenticationScheme),
            TokenForbidResult r     => Results.Forbid(r.Properties, r.AuthenticationSchemes),
            TokenBadRequestResult r => Results.BadRequest(r.Error),
            _ => Results.StatusCode(500)
        };
    }

    public static IResult FromResult(Result<LogoutCommandResult> result)
    {
        if (result.IsSuccess == false)
        {
            return Results.Problem(result.Message);
        }

        return result.Value switch
        {
            LogoutSignOutResult r  => Results.SignOut(r.Properties, r.AuthenticationSchemes),
            LogoutRedirectResult r => Results.Redirect(r.Url),
            _ => Results.StatusCode(500)
        };
    }

    public static IResult FromResult(Result<UserInfoCommandResult> result)
    {
        if (result.IsSuccess == false)
        {
            return Results.Problem(result.Message);
        }

        return result.Value switch
        {
            UserInfoJsonResult r      => Results.Json(r.Data),
            UserInfoChallengeResult r => Results.Challenge(r.Properties, r.AuthenticationSchemes),
            _ => Results.StatusCode(500)
        };
    }
}
