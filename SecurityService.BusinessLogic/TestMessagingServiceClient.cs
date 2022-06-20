namespace SecurityService.BusinessLogic;

using System;
using System.Threading;
using System.Threading.Tasks;
using MessagingService.Client;
using MessagingService.DataTransferObjects;

public class TestMessagingServiceClient : IMessagingServiceClient
{
    public SendEmailRequest LastEmailRequest { get; private set; } 
    public SendSMSRequest LastSMSRequest { get; private set; }

    public async Task<SendEmailResponse> SendEmail(String accessToken,
                                                   SendEmailRequest request,
                                                   CancellationToken cancellationToken) {
        this.LastEmailRequest = request;
        return new SendEmailResponse {
                                         MessageId = Guid.NewGuid()
                                     };
    }

    public async Task<SendSMSResponse> SendSMS(String accessToken,
                                               SendSMSRequest request,
                                               CancellationToken cancellationToken) {
        this.LastSMSRequest = request;
        return new SendSMSResponse
               {
                   MessageId = Guid.NewGuid()
               };
    }
}