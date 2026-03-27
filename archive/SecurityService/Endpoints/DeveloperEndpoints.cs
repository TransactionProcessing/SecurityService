using Microsoft.AspNetCore.Routing;

namespace SecurityService.Endpoints
{
    using Microsoft.AspNetCore.Builder;

    public static class DeveloperEndpoints
    {
        public static void MapDeveloperEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("api/developer/lastemail", Handlers.DeveloperHandler.GetLastEmailMessage)
                     .WithName("GetLastEmailMessage");

            endpoints.MapGet("api/developer/lastsms", Handlers.DeveloperHandler.GetLastSMSMessage)
                     .WithName("GetLastSMSMessage");
        }
    }
}