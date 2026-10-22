namespace SecurityService.Endpoints;

using SecurityService.OpenApi;

public static class OidcEndpoints
{
    private const string OpenIdConnectTag = "OpenID Connect";

    public static void MapOidcEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods("/connect/authorize", [HttpMethods.Get, HttpMethods.Post], Handlers.AuthorizeHandler.AuthorizeAsync)
            .WithName("Authorize")
            .WithTags(OpenIdConnectTag)
            .WithSummary("Start an OpenID Connect authorization request")
            .WithDescription("Interactive OIDC authorization endpoint. Send the standard authorization parameters as a query string for GET or as form fields for POST. Use `response_type=code` and PKCE with `code_challenge_method=S256` for the authorization-code flow. The user may be redirected to sign in, consent, or the registered client callback. The client and redirect URI must be registered.")
            .Produces(302).Produces(400).Produces(401);
        endpoints.MapPost("/connect/token", Handlers.TokenHandler.TokenAsync)
            .WithName("Token")
            .WithTags(OpenIdConnectTag)
            .WithSummary("Exchange an OAuth grant for tokens")
            .WithDescription("OAuth 2.0 token endpoint. Submit one grant as `application/x-www-form-urlencoded`; authenticate the client as required by its registration. Successful responses follow OAuth token response conventions (`access_token`, `token_type`, `expires_in`, and, when issued, `refresh_token`). Invalid grants return an OAuth error response such as `invalid_grant`.")
            .Produces(200, contentType: "application/json").Produces(400, contentType: "application/json").Produces(401)
            .WithResponseExample("""{"access_token":"eyJhbGciOi...","token_type":"Bearer","expires_in":3600,"refresh_token":"def50200..."}""");
        endpoints.MapMethods("/connect/logout", [HttpMethods.Get, HttpMethods.Post], Handlers.LogoutHandler.LogoutAsync)
            .WithName("Logout")
            .WithTags(OpenIdConnectTag)
            .WithSummary("End an OpenID Connect session")
            .WithDescription("OIDC end-session endpoint. Accepts the standard `id_token_hint`, `post_logout_redirect_uri`, and `state` parameters as a query string for GET or as form fields for POST. The post-logout redirect URI must be registered for the client.")
            .Produces(302).Produces(400);
        endpoints.MapMethods("/connect/userinfo", [HttpMethods.Get, HttpMethods.Post], Handlers.UserInfoHandler.UserInfoAsync)
            .WithName("UserInfo")
            .WithTags(OpenIdConnectTag)
            .WithSummary("Get claims for the authenticated user")
            .WithDescription("OIDC UserInfo endpoint. Send a valid access token in `Authorization: Bearer <access_token>` (GET or POST). Returns the claims authorized by the token's scopes; missing or invalid tokens are challenged.")
            .Produces(200, contentType: "application/json").Produces(401)
            .WithResponseExample("""{"sub":"7a0c1d2e-3456-4789-a123-123456789abc","name":"Avery Nguyen","given_name":"Avery","family_name":"Nguyen","email":"avery@example.com","email_verified":true}""");
    }
}
