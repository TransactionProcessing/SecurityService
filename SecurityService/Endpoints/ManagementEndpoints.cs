namespace SecurityService.Endpoints;

public static class ManagementEndpoints
{
    public static void MapManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/clients", Handlers.ClientHandler.CreateClient).WithName("CreateClient");
        endpoints.MapGet("/api/clients/{clientId}", Handlers.ClientHandler.GetClient).WithName("GetClient");
        endpoints.MapGet("/api/clients", Handlers.ClientHandler.GetClients).WithName("GetClients");

        endpoints.MapPost("/api/apiscopes", Handlers.ApiScopeHandler.CreateApiScope).WithName("CreateApiScope");
        endpoints.MapGet("/api/apiscopes/{name}", Handlers.ApiScopeHandler.GetApiScope).WithName("GetApiScope");
        endpoints.MapGet("/api/apiscopes", Handlers.ApiScopeHandler.GetApiScopes).WithName("GetApiScopes");

        endpoints.MapPost("/api/apiresources", Handlers.ApiResourceHandler.CreateApiResource).WithName("CreateApiResource");
        endpoints.MapGet("/api/apiresources/{name}", Handlers.ApiResourceHandler.GetApiResource).WithName("GetApiResource");
        endpoints.MapGet("/api/apiresources", Handlers.ApiResourceHandler.GetApiResources).WithName("GetApiResources");

        endpoints.MapPost("/api/identityresources", Handlers.IdentityResourceHandler.CreateIdentityResource).WithName("CreateIdentityResource");
        endpoints.MapGet("/api/identityresources/{name}", Handlers.IdentityResourceHandler.GetIdentityResource).WithName("GetIdentityResource");
        endpoints.MapGet("/api/identityresources", Handlers.IdentityResourceHandler.GetIdentityResources).WithName("GetIdentityResources");

        endpoints.MapPost("/api/roles", Handlers.RoleHandler.CreateRole).WithName("CreateRole");
        endpoints.MapGet("/api/roles/{roleId}", Handlers.RoleHandler.GetRole).WithName("GetRole");
        endpoints.MapGet("/api/roles", Handlers.RoleHandler.GetRoles).WithName("GetRoles");

        endpoints.MapPost("/api/users", Handlers.UserHandler.CreateUser).WithName("CreateUser");
        endpoints.MapGet("/api/users/{userId}", Handlers.UserHandler.GetUser).WithName("GetUser");
        endpoints.MapGet("/api/users", Handlers.UserHandler.GetUsers).WithName("GetUsers");
    }
}
