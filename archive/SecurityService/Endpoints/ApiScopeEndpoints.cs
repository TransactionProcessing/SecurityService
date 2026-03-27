using Microsoft.AspNetCore.Routing;

namespace SecurityService.Endpoints
{
    using Microsoft.AspNetCore.Builder;

    public static class ApiScopeEndpoints
    {
        public static void MapApiScopeEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("api/apiscopes", Handlers.ApiScopeHandler.CreateApiScope)
                     .WithName("CreateApiScope");

            endpoints.MapGet("api/apiscopes/{apiScopeName}", Handlers.ApiScopeHandler.GetApiScope)
                     .WithName("GetApiScope");

            endpoints.MapGet("api/apiscopes", Handlers.ApiScopeHandler.GetApiScopes)
                     .WithName("GetApiScopes");
        }
    }
}