using SecurityService.BusinessLogic.Oidc;
using TransactionProcessing.UnionResults;
using Results = Microsoft.AspNetCore.Http.Results;

namespace SecurityService.Factories;

public static class OidcResponseFactory
{
    public static IResult FromResult<T>(Result<T> result)
    {
        if (result.Value is not Success<T> success) {
            return TranslateResultError(result.GetErrors());
        }

        return success.Value switch
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

    private static IResult TranslateResultError(IReadOnlyList<Error> errors)
    {
        ErrorType errorType = errors.Count == 0 ? ErrorType.Unknown : errors[0].ErrorType;
        string[] messages = errors.Select(error => error.Message).ToArray();
        object errorResponse = new { Errors = messages };

        return errorType switch
        {
            ErrorType.Validation => Results.BadRequest(errorResponse),
            ErrorType.NotFound => Results.NotFound(errorResponse),
            ErrorType.Unauthorized => Results.Unauthorized(),
            ErrorType.Conflict => Results.Conflict(errorResponse),
            ErrorType.Failure or ErrorType.Critical => Results.InternalServerError(errorResponse),
            ErrorType.Forbidden => Results.Forbid(),
            _ => Results.StatusCode(501)
        };
    }
}
