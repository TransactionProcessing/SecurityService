using Microsoft.AspNetCore.Routing;

namespace SecurityService.Endpoints
{
    using Microsoft.AspNetCore.Builder;

    public static class ApiResourceEndpoints
    {
        public static void MapApiResourceEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("api/apiresources", Handlers.ApiResourceHandler.CreateApiResource)
                     .WithName("CreateApiResource");

            endpoints.MapGet("api/apiresources/{apiResourceName}", Handlers.ApiResourceHandler.GetApiResource)
                     .WithName("GetApiResource");

            endpoints.MapGet("api/apiresources", Handlers.ApiResourceHandler.GetApiResources)
                     .WithName("GetApiResources");
        }
    }
}