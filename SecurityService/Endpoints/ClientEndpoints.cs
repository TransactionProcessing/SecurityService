using Microsoft.AspNetCore.Routing;

namespace SecurityService.Endpoints
{
    using Microsoft.AspNetCore.Builder;

    public static class ClientEndpoints
    {
        public static void MapClientEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("api/clients", Handlers.ClientHandler.CreateClient)
                     .WithName("CreateClient");

            endpoints.MapGet("api/clients/{clientId}", Handlers.ClientHandler.GetClient)
                     .WithName("GetClient");

            endpoints.MapGet("api/clients", Handlers.ClientHandler.GetClients)
                     .WithName("GetClients");
        }
    }
}