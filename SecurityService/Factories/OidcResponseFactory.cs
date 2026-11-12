using Microsoft.AspNetCore.Authentication;
using SecurityService.BusinessLogic.Oidc;
using TransactionProcessing.UnionResults;
using System.Security.Claims;
using TransactionProcessing.UnionResults.AspNetCore;

namespace SecurityService.Factories;

public static class OidcResponseFactory
{
    public static IResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess == false) {
            return TranslateResultError(result);
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

    internal static IResult TranslateResultError(IResultOutcome result)
    {
        ResultOutcome outcome = result.GetOutcome();
        ErrorType errorType = outcome.Error?.ErrorType ?? outcome.Errors?.ErrorType ?? ErrorType.Unknown;
        string[] messages = outcome.Error is not null
            ? [outcome.Error.Message]
            : outcome.Errors?.Select(error => error.Message).ToArray() ?? [];
        object errorResponse = new { Errors = messages };

        return errorType switch
        {
            ErrorType.Validation => Microsoft.AspNetCore.Http.Results.BadRequest(errorResponse),
            ErrorType.NotFound => Microsoft.AspNetCore.Http.Results.NotFound(errorResponse),
            ErrorType.Unauthorized => Microsoft.AspNetCore.Http.Results.Unauthorized(),
            ErrorType.Conflict => Microsoft.AspNetCore.Http.Results.Conflict(errorResponse),
            ErrorType.Failure or ErrorType.Critical => Microsoft.AspNetCore.Http.Results.InternalServerError(errorResponse),
            ErrorType.Forbidden => Microsoft.AspNetCore.Http.Results.Forbid(),
            _ => Microsoft.AspNetCore.Http.Results.StatusCode(501)
        };
    }
}
