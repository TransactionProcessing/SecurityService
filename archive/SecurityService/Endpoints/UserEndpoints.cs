using Microsoft.AspNetCore.Routing;

namespace SecurityService.Endpoints
{
    using Microsoft.AspNetCore.Builder;

    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("api/users", Handlers.UserHandler.CreateUser)
                     .WithName("CreateUser");

            endpoints.MapGet("api/users/{userId:guid}", Handlers.UserHandler.GetUser)
                     .WithName("GetUser");

            endpoints.MapGet("api/users", Handlers.UserHandler.GetUsers)
                     .WithName("GetUsers");
        }
    }
}