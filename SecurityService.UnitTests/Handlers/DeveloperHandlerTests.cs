using MessagingService.DataTransferObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using SecurityService.BusinessLogic;
using SecurityService.Handlers;
using Shouldly;

namespace SecurityService.UnitTests.Handlers;

public class DeveloperHandlerTests
{
    [Fact]
    public async Task GetLastEmail_WhenEmailCaptured_ReturnsSnapshot()
    {
        var store = new TestMessagingServiceClient();
        await store.SendEmail("", new SendEmailRequest() { Body = "Body", Subject = "Subject", ToAddresses = new List<string>() { "user@example.com" } }, CancellationToken.None);

        var result = DeveloperHandler.GetLastEmail(store);
        var okResult = result.ShouldBeOfType<Ok<SendEmailRequest>>();

        okResult.Value.ShouldNotBeNull();
        okResult.Value.ToAddresses.ShouldContain("user@example.com");
        okResult.Value.Subject.ShouldBe("Subject");
        okResult.Value.Body.ShouldBe("Body");
    }
}
