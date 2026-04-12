using MessagingService.Client;
using MessagingService.DataTransferObjects;
using SecurityService.BusinessLogic;

namespace SecurityService.Handlers;

public static class DeveloperHandler
{
    public static IResult GetLastEmail(IMessagingServiceClient messagingServiceClient) {
        if (messagingServiceClient.GetType() == typeof(TestMessagingServiceClient))
        {
            SendEmailRequest lastEmailRequest = ((TestMessagingServiceClient)messagingServiceClient).LastEmailRequest;
            return Results.Ok(lastEmailRequest);
        }
        return Results.NotFound();
    }

    public static IResult GetLastSms(IMessagingServiceClient messagingServiceClient) {
        if (messagingServiceClient.GetType() == typeof(TestMessagingServiceClient)) {
            SendSMSRequest lastSMSRequest = ((TestMessagingServiceClient)messagingServiceClient).LastSMSRequest;
            return Results.Ok(lastSMSRequest);
        }

        return Results.NotFound();
    }
}
