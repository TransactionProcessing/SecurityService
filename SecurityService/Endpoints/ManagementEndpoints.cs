namespace SecurityService.Endpoints;

using SecurityService.Authorization;

public static class ManagementEndpoints
{
    public static void MapManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder management = endpoints.MapGroup("/api")
            .RequireAuthorization(ManagementAuthorizationPolicies.ManagementApi);

        management.MapPost("/clients", Handlers.ClientHandler.CreateClient).WithName("CreateClient");
        management.MapGet("/clients/{clientId}", Handlers.ClientHandler.GetClient).WithName("GetClient");
        management.MapGet("/clients", Handlers.ClientHandler.GetClients).WithName("GetClients");

        management.MapPost("/apiscopes", Handlers.ApiScopeHandler.CreateApiScope).WithName("CreateApiScope");
        management.MapGet("/apiscopes/{name}", Handlers.ApiScopeHandler.GetApiScope).WithName("GetApiScope");
        management.MapGet("/apiscopes", Handlers.ApiScopeHandler.GetApiScopes).WithName("GetApiScopes");

        management.MapPost("/apiresources", Handlers.ApiResourceHandler.CreateApiResource).WithName("CreateApiResource");
        management.MapGet("/apiresources/{name}", Handlers.ApiResourceHandler.GetApiResource).WithName("GetApiResource");
        management.MapGet("/apiresources", Handlers.ApiResourceHandler.GetApiResources).WithName("GetApiResources");

        management.MapPost("/identityresources", Handlers.IdentityResourceHandler.CreateIdentityResource).WithName("CreateIdentityResource");
        management.MapGet("/identityresources/{name}", Handlers.IdentityResourceHandler.GetIdentityResource).WithName("GetIdentityResource");
        management.MapGet("/identityresources", Handlers.IdentityResourceHandler.GetIdentityResources).WithName("GetIdentityResources");

        management.MapPost("/roles", Handlers.RoleHandler.CreateRole).WithName("CreateRole");
        management.MapGet("/roles/{roleId}", Handlers.RoleHandler.GetRole).WithName("GetRole");
        management.MapGet("/roles", Handlers.RoleHandler.GetRoles).WithName("GetRoles");

        management.MapPost("/users", Handlers.UserHandler.CreateUser).WithName("CreateUser");
        management.MapGet("/users/{userId}", Handlers.UserHandler.GetUser).WithName("GetUser");
        management.MapGet("/users", Handlers.UserHandler.GetUsers).WithName("GetUsers");
        management.MapPut("/users/{userId}/mfa-policy", Handlers.UserHandler.RequireMfa).WithName("RequireUserMfa");
        management.MapDelete("/users/{userId}/mfa-policy", Handlers.UserHandler.RemoveMfa).WithName("RemoveUserMfa");
        management.MapPut("/roles/{roleId}/mfa-policy", Handlers.RoleHandler.RequireMfa).WithName("RequireRoleMfa");
        management.MapDelete("/roles/{roleId}/mfa-policy", Handlers.RoleHandler.RemoveMfa).WithName("RemoveRoleMfa");
    }
}
