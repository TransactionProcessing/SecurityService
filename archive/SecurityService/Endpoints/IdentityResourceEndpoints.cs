using Microsoft.AspNetCore.Routing;

namespace SecurityService.Endpoints
{
    using Microsoft.AspNetCore.Builder;

    public static class IdentityResourceEndpoints
    {
        public static void MapIdentityResourceEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("api/identityresources", Handlers.IdentityResourceHandler.CreateIdentityResource)
                     .WithName("CreateIdentityResource");


            endpoints.MapGet("api/identityresources/{apiResourceName}", Handlers.IdentityResourceHandler.GetIdentityResource)
                .WithName("GetIdentityResource");

            endpoints.MapGet("api/identityresources", Handlers.IdentityResourceHandler.GetIdentityResources)
                .WithName("GetIdentityResources");
        }
    }
}