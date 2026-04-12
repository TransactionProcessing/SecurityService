using Microsoft.AspNetCore.Authentication;
using SecurityService.BusinessLogic.Oidc;
using SimpleResults;
using System.Net;
using System.Security.Claims;
using Shared.Results.Web;

namespace SecurityService.Factories;

public static class OidcResponseFactory
{
    public static IResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess == false) {
            return TranslateResultStatus(result);
        }

        return result.Data switch
        {
            AuthorizeSignInResult r     => Results.SignIn(r.Principal, properties: null, authenticationScheme: r.AuthenticationScheme),
            AuthorizeRedirectResult r   => Results.Redirect(r.Url),
            AuthorizeForbidResult r     => Results.Forbid(r.Properties, r.AuthenticationSchemes),
            AuthorizeChallengeResult r  => Results.Challenge(r.Properties, r.AuthenticationSchemes),
            AuthorizeBadRequestResult r => Results.BadRequest(r.Error),
            TokenSignInResult r => Results.SignIn(r.Principal, properties: null, authenticationScheme: r.AuthenticationScheme),
            TokenForbidResult r => Results.Forbid(r.Properties, r.AuthenticationSchemes),
            TokenBadRequestResult r => Results.BadRequest(r.Error),
            LogoutSignOutResult r => Results.SignOut(r.Properties, r.AuthenticationSchemes),
            LogoutRedirectResult r => Results.Redirect(r.Url),
            UserInfoJsonResult r => Results.Json(r.Data),
            UserInfoChallengeResult r => Results.Challenge(r.Properties, r.AuthenticationSchemes),
            _ => Results.StatusCode(500)
        };
    }

    internal static IResult TranslateResultStatus(ResultBase result)
    {
        ErrorResponse errorResponse = new()
        {
            Errors = result.Errors.Any() switch
            {
                true => result.Errors.ToList(),
                _ => [result.Message],
            }
        };

        return result.Status switch
        {
            ResultStatus.Invalid => Microsoft.AspNetCore.Http.Results.BadRequest(errorResponse),
            ResultStatus.NotFound => Microsoft.AspNetCore.Http.Results.NotFound(errorResponse),
            ResultStatus.Unauthorized => Microsoft.AspNetCore.Http.Results.Unauthorized(),
            ResultStatus.Conflict => Microsoft.AspNetCore.Http.Results.Conflict(errorResponse),
            ResultStatus.Failure => Microsoft.AspNetCore.Http.Results.InternalServerError(errorResponse),
            ResultStatus.CriticalError => Microsoft.AspNetCore.Http.Results.InternalServerError(errorResponse),
            ResultStatus.Forbidden => Microsoft.AspNetCore.Http.Results.Forbid(),
            _ => Microsoft.AspNetCore.Http.Results.StatusCode((int)HttpStatusCode.NotImplemented)
        };
    }

    //public static IResult FromResult(Result<TokenCommandResult> result)
    //{
    //    if (result.IsSuccess == false)
    //    {
    //        return TranslateResultStatus(result);
    //    }

    //    return result.Data switch
    //    {
    //        TokenSignInResult r     => Results.SignIn(r.Principal, properties: null, authenticationScheme: r.AuthenticationScheme),
    //        TokenForbidResult r     => Results.Forbid(r.Properties, r.AuthenticationSchemes),
    //        TokenBadRequestResult r => Results.BadRequest(r.Error),
    //        _ => Results.StatusCode(500)
    //    };
    //}

    //public static IResult FromResult(Result<LogoutCommandResult> result)
    //{
    //    if (result.IsSuccess == false)
    //    {
    //        return TranslateResultStatus(result);
    //    }

    //    return result.Data switch
    //    {
    //        LogoutSignOutResult r  => Results.SignOut(r.Properties, r.AuthenticationSchemes),
    //        LogoutRedirectResult r => Results.Redirect(r.Url),
    //        _ => Results.StatusCode(500)
    //    };
    //}

    //public static IResult FromResult(Result<UserInfoCommandResult> result)
    //{
    //    if (result.IsSuccess == false)
    //    {
    //        return TranslateResultStatus(result);
    //    }

    //    return result.Data switch
    //    {
    //        UserInfoJsonResult r      => Results.Json(r.Data),
    //        UserInfoChallengeResult r => Results.Challenge(r.Properties, r.AuthenticationSchemes),
    //        _ => Results.StatusCode(500)
    //    };
    //}
}
