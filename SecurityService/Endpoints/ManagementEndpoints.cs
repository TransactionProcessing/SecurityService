namespace SecurityService.Endpoints;

using SecurityService.Authorization;
using SecurityService.OpenApi;

public static class ManagementEndpoints
{
    public static void MapManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder management = endpoints.MapGroup("/api")
            .WithTags("Management")
            .RequireAuthorization(ManagementAuthorizationPolicies.ManagementApi);

        management.MapPost("/clients", Handlers.ClientHandler.CreateClient).WithName("CreateClient")
            .WithSummary("Register an OAuth client")
            .WithDescription("Creates a client registration used by OAuth/OIDC flows. Supply allowed grant types, scopes, and exact callback URLs. A secret is used for confidential clients and is never returned by the read endpoints. Requires management API authorization. Invalid input returns 400; duplicate IDs may return 409.")
            .WithRequestExample("""{"clientId":"orders-web","secret":"replace-with-a-secret","clientName":"Orders web app","allowedScopes":["openid","profile","orders.read"],"allowedGrantTypes":["authorization_code","refresh_token"],"clientUri":"https://orders.example.com","clientRedirectUris":["https://orders.example.com/signin-oidc"],"clientPostLogoutRedirectUris":["https://orders.example.com/signed-out"],"requireConsent":true,"allowOfflineAccess":true}""")
            .Produces(200).Produces(400).Produces(401).Produces(403).Produces(404).Produces(409).Produces(500);
        management.MapGet("/clients/{clientId}", Handlers.ClientHandler.GetClient).WithName("GetClient")
            .WithSummary("Get a client registration")
            .WithDescription("Returns the public configuration for one registered OAuth client. The client secret is never included. The caller must have management API authorization. Returns 404 when the ID is unknown.")
            .Produces<SecurityService.DataTransferObjects.ClientResponse>()
            .Produces(404).Produces(401).Produces(403)
            .WithResponseExample("""{"clientId":"orders-web","clientName":"Orders web app","allowedScopes":["openid","orders.read"],"allowedGrantTypes":["authorization_code"],"clientRedirectUris":["https://orders.example.com/signin-oidc"],"clientPostLogoutRedirectUris":[],"requireConsent":true,"allowOfflineAccess":false,"clientType":"confidential"}""");
        management.MapGet("/clients", Handlers.ClientHandler.GetClients).WithName("GetClients")
            .WithSummary("List client registrations")
            .WithDescription("Returns all registered OAuth clients as an array. No pagination or query filters are currently supported. Client secrets are never included.")
            .Produces<List<SecurityService.DataTransferObjects.ClientResponse>>()
            .Produces(401).Produces(403)
            .WithResponseExample("""[{"clientId":"orders-web","clientName":"Orders web app","allowedScopes":["openid","orders.read"],"allowedGrantTypes":["authorization_code"],"clientRedirectUris":["https://orders.example.com/signin-oidc"],"clientPostLogoutRedirectUris":[],"requireConsent":true,"allowOfflineAccess":false,"clientType":"confidential"}]""");

        management.MapPost("/apiscopes", Handlers.ApiScopeHandler.CreateApiScope).WithName("CreateApiScope")
            .WithSummary("Create an API scope")
            .WithDescription("Registers a named permission that clients can request in access tokens. Requires management API authorization. Invalid input returns 400; a duplicate name may return 409.")
            .WithRequestExample("""{"name":"orders.read","displayName":"Read orders","description":"View orders belonging to the signed-in user."}""")
            .Produces(200).Produces(400).Produces(401).Produces(403).Produces(404).Produces(409).Produces(500);
        management.MapGet("/apiscopes/{name}", Handlers.ApiScopeHandler.GetApiScope).WithName("GetApiScope")
            .WithSummary("Get an API scope")
            .WithDescription("Returns one API scope by its exact, case-sensitive configured name. Returns 404 when no scope has that name. Requires management API authorization.")
            .Produces<SecurityService.DataTransferObjects.ApiScopeResponse>().Produces(404).Produces(401).Produces(403)
            .WithResponseExample("""{"name":"orders.read","displayName":"Read orders","description":"View orders belonging to the signed-in user."}""");
        management.MapGet("/apiscopes", Handlers.ApiScopeHandler.GetApiScopes).WithName("GetApiScopes")
            .WithSummary("List API scopes")
            .WithDescription("Returns every configured API scope as an array. No pagination or query filters are currently supported. Requires management API authorization.")
            .Produces<List<SecurityService.DataTransferObjects.ApiScopeResponse>>().Produces(401).Produces(403)
            .WithResponseExample("""[{"name":"orders.read","displayName":"Read orders","description":"View orders belonging to the signed-in user."}]""");

        management.MapPost("/apiresources", Handlers.ApiResourceHandler.CreateApiResource).WithName("CreateApiResource")
            .WithSummary("Create an API resource")
            .WithDescription("Registers a protected API and associates it with scopes and user claims. Requires management API authorization. Invalid input returns 400; a duplicate name may return 409.")
            .WithRequestExample("""{"name":"orders-api","displayName":"Orders API","description":"Order management service","secret":"replace-with-a-secret","scopes":["orders.read"],"userClaims":["sub","email"]}""")
            .Produces(200).Produces(400).Produces(401).Produces(403).Produces(404).Produces(409).Produces(500);
        management.MapGet("/apiresources/{name}", Handlers.ApiResourceHandler.GetApiResource).WithName("GetApiResource")
            .WithSummary("Get an API resource")
            .WithDescription("Returns one API resource by its exact configured name. The resource secret is not returned. Returns 404 when unknown. Requires management API authorization.")
            .Produces<SecurityService.DataTransferObjects.ApiResourceResponse>().Produces(404).Produces(401).Produces(403)
            .WithResponseExample("""{"name":"orders-api","displayName":"Orders API","description":"Order management service","scopes":["orders.read"],"userClaims":["sub","email"]}""");
        management.MapGet("/apiresources", Handlers.ApiResourceHandler.GetApiResources).WithName("GetApiResources")
            .WithSummary("List API resources")
            .WithDescription("Returns all configured API resources as an array. Resource secrets are not returned. No pagination or query filters are currently supported.")
            .Produces<List<SecurityService.DataTransferObjects.ApiResourceResponse>>().Produces(401).Produces(403)
            .WithResponseExample("""[{"name":"orders-api","displayName":"Orders API","description":"Order management service","scopes":["orders.read"],"userClaims":["sub","email"]}]""");

        management.MapPost("/identityresources", Handlers.IdentityResourceHandler.CreateIdentityResource).WithName("CreateIdentityResource")
            .WithSummary("Create an identity resource")
            .WithDescription("Registers an OIDC identity resource and the claims it exposes. Requires management API authorization.")
            .WithRequestExample("""{"name":"profile","displayName":"Profile","description":"Basic profile claims","required":false,"emphasize":false,"showInDiscoveryDocument":true,"claims":["name","given_name","family_name"]}""")
            .Produces(200).Produces(400).Produces(401).Produces(403).Produces(404).Produces(409).Produces(500);
        management.MapGet("/identityresources/{name}", Handlers.IdentityResourceHandler.GetIdentityResource).WithName("GetIdentityResource")
            .WithSummary("Get an identity resource")
            .WithDescription("Returns one OIDC identity resource by its exact configured name. Returns 404 when unknown. Requires management API authorization.")
            .Produces<SecurityService.DataTransferObjects.IdentityResourceResponse>().Produces(404).Produces(401).Produces(403)
            .WithResponseExample("""{"name":"profile","displayName":"Profile","description":"Basic profile claims","required":false,"emphasize":false,"showInDiscoveryDocument":true,"claims":["name","given_name","family_name"]}""");
        management.MapGet("/identityresources", Handlers.IdentityResourceHandler.GetIdentityResources).WithName("GetIdentityResources")
            .WithSummary("List identity resources")
            .WithDescription("Returns all configured OIDC identity resources as an array. No pagination or query filters are currently supported.")
            .Produces<List<SecurityService.DataTransferObjects.IdentityResourceResponse>>().Produces(401).Produces(403)
            .WithResponseExample("""[{"name":"profile","displayName":"Profile","description":"Basic profile claims","required":false,"emphasize":false,"showInDiscoveryDocument":true,"claims":["name","given_name","family_name"]}]""");

        management.MapPost("/roles", Handlers.RoleHandler.CreateRole).WithName("CreateRole")
            .WithSummary("Create a role")
            .WithDescription("Creates a role that can be assigned to users and used in authorization decisions. Requires management API authorization.")
            .WithRequestExample("""{"roleName":"OrderManager"}""")
            .Produces(200).Produces(400).Produces(401).Produces(403).Produces(404).Produces(409).Produces(500);
        management.MapGet("/roles/{roleId}", Handlers.RoleHandler.GetRole).WithName("GetRole")
            .WithSummary("Get a role")
            .WithDescription("Returns a role by its identity role ID (not its display name). Returns 404 when unknown. Requires management API authorization.")
            .Produces<SecurityService.DataTransferObjects.RoleResponse>().Produces(404).Produces(401).Produces(403)
            .WithResponseExample("""{"roleId":"2f0c1d2e-3456-4789-a123-123456789abc","roleName":"OrderManager"}""");
        management.MapGet("/roles", Handlers.RoleHandler.GetRoles).WithName("GetRoles")
            .WithSummary("List roles")
            .WithDescription("Returns every role as an array. No pagination or query filters are currently supported. Requires management API authorization.")
            .Produces<List<SecurityService.DataTransferObjects.RoleResponse>>().Produces(401).Produces(403)
            .WithResponseExample("""[{"roleId":"2f0c1d2e-3456-4789-a123-123456789abc","roleName":"OrderManager"}]""");

        management.MapPost("/users", Handlers.UserHandler.CreateUser).WithName("CreateUser")
            .WithSummary("Create a user")
            .WithDescription("Creates a local account and assigns the supplied claims and roles. Password must satisfy the configured identity password policy. Requires management API authorization.")
            .WithRequestExample("""{"givenName":"Avery","familyName":"Nguyen","userName":"avery.nguyen","password":"replace-with-a-policy-compliant-password","emailAddress":"avery@example.com","phoneNumber":"+15550102020","claims":{"department":"sales"},"roles":["OrderManager"]}""")
            .Produces(200).Produces(400).Produces(401).Produces(403).Produces(404).Produces(409).Produces(500);
        management.MapGet("/users/{userId}", Handlers.UserHandler.GetUser).WithName("GetUser")
            .WithSummary("Get a user")
            .WithDescription("Returns the account, claims, roles, and registration time for the specified identity user ID. Returns 404 when unknown. Requires management API authorization.")
            .Produces<SecurityService.DataTransferObjects.UserResponse>().Produces(404).Produces(401).Produces(403)
            .WithResponseExample("""{"userId":"7a0c1d2e-3456-4789-a123-123456789abc","userName":"avery.nguyen","emailAddress":"avery@example.com","phoneNumber":"+15550102020","givenName":"Avery","middleName":null,"familyName":"Nguyen","claims":{"department":"sales"},"roles":["OrderManager"],"registrationDateTime":"2026-10-22T10:30:00Z"}""");
        management.MapGet("/users", Handlers.UserHandler.GetUsers).WithName("GetUsers")
            .WithSummary("List users")
            .WithDescription("Returns users as an array. Optionally filter by exact user name using `?userName=avery.nguyen`; omit it to return all users. No pagination is currently supported. Requires management API authorization.")
            .Produces<List<SecurityService.DataTransferObjects.UserResponse>>().Produces(401).Produces(403)
            .WithResponseExample("""[{"userId":"7a0c1d2e-3456-4789-a123-123456789abc","userName":"avery.nguyen","emailAddress":"avery@example.com","phoneNumber":"+15550102020","givenName":"Avery","middleName":null,"familyName":"Nguyen","claims":{"department":"sales"},"roles":["OrderManager"],"registrationDateTime":"2026-10-22T10:30:00Z"}]""");
        management.MapPut("/users/{userId}/mfa-policy", Handlers.UserHandler.RequireMfa).WithName("RequireUserMfa")
            .WithSummary("Require MFA for a user").WithDescription("Enables the MFA-required policy for the specified user. Example: `PUT /api/users/7a0c1d2e-3456-4789-a123-123456789abc/mfa-policy`. Requires management API authorization.").Produces(200).Produces(404).Produces(401).Produces(403);
        management.MapDelete("/users/{userId}/mfa-policy", Handlers.UserHandler.RemoveMfa).WithName("RemoveUserMfa")
            .WithSummary("Remove the user MFA requirement").WithDescription("Removes the MFA-required policy from the specified user. Example: `DELETE /api/users/7a0c1d2e-3456-4789-a123-123456789abc/mfa-policy`. Requires management API authorization.").Produces(200).Produces(404).Produces(401).Produces(403);
        management.MapPut("/roles/{roleId}/mfa-policy", Handlers.RoleHandler.RequireMfa).WithName("RequireRoleMfa")
            .WithSummary("Require MFA for a role").WithDescription("Enables the MFA-required policy for the specified role; users in that role must satisfy MFA according to the service policy. Example: `PUT /api/roles/role-id/mfa-policy`. Requires management API authorization.").Produces(200).Produces(404).Produces(401).Produces(403);
        management.MapDelete("/roles/{roleId}/mfa-policy", Handlers.RoleHandler.RemoveMfa).WithName("RemoveRoleMfa")
            .WithSummary("Remove the role MFA requirement").WithDescription("Removes the MFA-required policy from the specified role. Example: `DELETE /api/roles/role-id/mfa-policy`. Requires management API authorization.").Produces(200).Produces(404).Produces(401).Produces(403);
    }
}
