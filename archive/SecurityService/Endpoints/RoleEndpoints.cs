using Microsoft.AspNetCore.Routing;

namespace SecurityService.Endpoints
{
    using Microsoft.AspNetCore.Builder;

    public static class RoleEndpoints
    {
        public static void MapRoleEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("api/roles", Handlers.RoleHandler.CreateRole)
                     .WithName("CreateRole");

            endpoints.MapGet("api/roles/{roleId:guid}", Handlers.RoleHandler.GetRole)
                     .WithName("GetRole");

            endpoints.MapGet("api/roles", Handlers.RoleHandler.GetRoles)
                     .WithName("GetRoles");
        }
    }
}