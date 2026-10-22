namespace SecurityService.Endpoints;

public static class DeveloperEndpoints
{
    public static void MapDeveloperEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/developer/lastemail", Handlers.DeveloperHandler.GetLastEmail).WithName("GetLastEmail").WithTags("Developer testing")
            .WithSummary("Get the most recent test email")
            .WithDescription("Integration-test helper that returns the last email captured by the test messaging client, including recipient, subject, and body. Example: `GET /api/developer/lastemail` returns the captured email request JSON. Only mapped in the IntegrationTest environment. Returns 404 when no test email has been captured or the test client is unavailable.")
            .Produces<MessagingService.DataTransferObjects.SendEmailRequest>().Produces(404);
        endpoints.MapGet("/api/developer/lastsms", Handlers.DeveloperHandler.GetLastSms).WithName("GetLastSms").WithTags("Developer testing")
            .WithSummary("Get the most recent test SMS")
            .WithDescription("Integration-test helper that returns the last SMS captured by the test messaging client, including recipient and message body. Example: `GET /api/developer/lastsms` returns the captured SMS request JSON. Only mapped in the IntegrationTest environment. Returns 404 when no test SMS has been captured or the test client is unavailable.")
            .Produces<MessagingService.DataTransferObjects.SendSMSRequest>().Produces(404);
    }
}
