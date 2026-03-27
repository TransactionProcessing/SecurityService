namespace SecurityService.Endpoints;

public static class DeveloperEndpoints
{
    public static void MapDeveloperEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/developer/lastemail", Handlers.DeveloperHandler.GetLastEmail).WithName("GetLastEmail");
        endpoints.MapGet("/api/developer/lastsms", Handlers.DeveloperHandler.GetLastSms).WithName("GetLastSms");
    }
}
