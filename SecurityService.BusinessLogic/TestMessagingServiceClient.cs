namespace SecurityService.BusinessLogic;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MessagingService.Client;
using MessagingService.DataTransferObjects;
using Shared.Logger;

[ExcludeFromCodeCoverage]
public class TestMessagingServiceClient : IMessagingServiceClient
{
    public SendEmailRequest LastEmailRequest { get; private set; } 
    public SendSMSRequest LastSMSRequest { get; private set; }

    public async Task ResendEmail(string accessToken, ResendEmailRequest request, CancellationToken cancellationToken)
    {
        
    }

    public async Task<SendEmailResponse> SendEmail(String accessToken,
                                                   SendEmailRequest request,
                                                   CancellationToken cancellationToken) {
        Logger.LogWarning($"Sending Email {request.Subject}");
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