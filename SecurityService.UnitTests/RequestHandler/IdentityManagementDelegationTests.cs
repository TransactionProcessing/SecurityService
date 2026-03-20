using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SecurityService.BusinessLogic.IdentityManagement;
using SecurityService.BusinessLogic.RequestHandlers;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Models;
using Shouldly;
using SimpleResults;
using Xunit;

namespace SecurityService.UnitTests.RequestHandler;

public class IdentityManagementDelegationTests
{
    [Fact]
    public async Task ClientRequestHandler_CreateClientCommand_DelegatesToIdentityManagementService()
    {
        Mock<IIdentityManagementService> identityManagementService = new Mock<IIdentityManagementService>();
        SecurityServiceCommands.CreateClientCommand command = TestData.CreateClientCommand;
        identityManagementService.Setup(s => s.CreateClient(command, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        ClientRequestHandler handler = new ClientRequestHandler(identityManagementService.Object);

        Result result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        identityManagementService.Verify(s => s.CreateClient(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserRequestHandler_GetUserQuery_DelegatesToIdentityManagementService()
    {
        Mock<IIdentityManagementService> identityManagementService = new Mock<IIdentityManagementService>();
        SecurityServiceQueries.GetUserQuery query = TestData.GetUserQuery;
        UserDetails userDetails = new UserDetails { UserId = query.UserId, Username = "user" };
        identityManagementService.Setup(s => s.GetUser(query, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(userDetails));
        UserRequestHandler handler = new UserRequestHandler(identityManagementService.Object);

        Result<UserDetails> result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.UserId.ShouldBe(query.UserId);
        identityManagementService.Verify(s => s.GetUser(query, It.IsAny<CancellationToken>()), Times.Once);
    }
}
