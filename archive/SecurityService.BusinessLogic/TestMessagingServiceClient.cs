using SimpleResults;

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

    public async Task<Result> ResendEmail(string accessToken, ResendEmailRequest request, CancellationToken cancellationToken) => Result.Success();

    public async Task<Result> SendEmail(String accessToken,
                                        SendEmailRequest request,
                                        CancellationToken cancellationToken) {
        Logger.LogWarning($"Sending Email {request.Subject}");
        this.LastEmailRequest = request;
        return Result.Success();
    }

    public async Task<Result> SendSMS(String accessToken,
                                      SendSMSRequest request,
                                      CancellationToken cancellationToken) {
        this.LastSMSRequest = request;
        return Result.Success();
    }
}