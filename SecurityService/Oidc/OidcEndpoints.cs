namespace SecurityService.Oidc;

public static class OidcEndpoints
{
    public static void MapOidcEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods("/connect/authorize", [HttpMethods.Get, HttpMethods.Post], Handlers.AuthorizeHandler.AuthorizeAsync);
        endpoints.MapPost("/connect/token", Handlers.TokenHandler.TokenAsync);
        endpoints.MapMethods("/connect/logout", [HttpMethods.Get, HttpMethods.Post], Handlers.LogoutHandler.LogoutAsync);
        endpoints.MapMethods("/connect/userinfo", [HttpMethods.Get, HttpMethods.Post], Handlers.UserInfoHandler.UserInfoAsync);
    }
}
