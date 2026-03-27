using MessagingService.Client;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SecurityService.BusinessLogic;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;

namespace SecurityService.UnitTests.RequestHandlers;

public class UserRequestHandlerTests
{
    [Fact]
    public async Task UserLifecycle_CreateGetAndList_Works()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.UserLifecycle_CreateGetAndList_Works));
        var mediator = provider.GetRequiredService<IMediator>();

        await mediator.Send(new SecurityServiceCommands.CreateRoleCommand("admin"));

        var createResult = await mediator.Send(new SecurityServiceCommands.CreateUserCommand(
            "Alice",
            null,
            "Tester",
            "alice",
            "Password1!",
            "alice@example.com",
            "1234567890",
            new Dictionary<string, string> { ["merchant_id"] = "1234" },
            ["admin"]));

        createResult.IsSuccess.ShouldBeTrue();

        var listResult = await mediator.Send(new SecurityServiceQueries.GetUsersQuery("ali"));
        listResult.Data.ShouldHaveSingleItem();

        var getResult = await mediator.Send(new SecurityServiceQueries.GetUserQuery(listResult.Data.First().UserId));
        getResult.IsSuccess.ShouldBeTrue();
        getResult.Data!.Claims["merchant_id"].ShouldBe("1234");

        
    }

    [Fact]
    public async Task CreateUser_WhenDuplicate_ReturnsConflict()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateUser_WhenDuplicate_ReturnsConflict));
        var mediator = provider.GetRequiredService<IMediator>();

        await mediator.Send(new SecurityServiceCommands.CreateRoleCommand("admin"));
        var command = new SecurityServiceCommands.CreateUserCommand(
            "Alice",
            null,
            "Tester",
            "alice",
            "Password1!",
            "alice@example.com",
            "1234567890",
            new Dictionary<string, string> { ["merchant_id"] = "1234" },
            ["admin"]);

        (await mediator.Send(command)).IsSuccess.ShouldBeTrue();
        var duplicateResult = await mediator.Send(command);

        duplicateResult.IsFailed.ShouldBeTrue();
        duplicateResult.Status.ShouldBe(SimpleResults.ResultStatus.Conflict);
    }

    [Fact]
    public async Task CreateUser_WhenRoleAssignmentFails_RollsBackCreatedUser()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateUser_WhenRoleAssignmentFails_RollsBackCreatedUser));
        var mediator = provider.GetRequiredService<IMediator>();

        var createResult = await mediator.Send(new SecurityServiceCommands.CreateUserCommand(
            "Alice",
            null,
            "Tester",
            "alice",
            "Password1!",
            "alice@example.com",
            "1234567890",
            new Dictionary<string, string> { ["merchant_id"] = "1234" },
            ["missing-role"]));

        createResult.IsFailed.ShouldBeTrue();

        var listResult = await mediator.Send(new SecurityServiceQueries.GetUsersQuery("alice"));
        listResult.Data.ShouldBeEmpty();
    }

    [Fact]
    public async Task ConfirmUserEmailAddress_UsesResolvedMediatorAndConfirmsUser()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.ConfirmEmailAsync(user, "token-123"))
            .ReturnsAsync(IdentityResult.Success);

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = TestServiceProviderFactory.Create(nameof(this.ConfirmUserEmailAddress_UsesResolvedMediatorAndConfirmsUser), userManager.Object, signInManager.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.ConfirmUserEmailAddressCommand("alice", "token-123"));

        result.IsSuccess.ShouldBeTrue();
        userManager.Verify(instance => instance.FindByNameAsync("alice"), Times.Once);
        userManager.Verify(instance => instance.ConfirmEmailAsync(user, "token-123"), Times.Once);
    }

    [Fact]
    public async Task ConfirmUserEmailAddress_WhenUserMissing_ReturnsNotFound()
    {
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync((ApplicationUser?)null);

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = TestServiceProviderFactory.Create(nameof(this.ConfirmUserEmailAddress_WhenUserMissing_ReturnsNotFound), userManager.Object, signInManager.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.ConfirmUserEmailAddressCommand("alice", "token-123"));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(SimpleResults.ResultStatus.NotFound);
    }

    [Fact]
    public async Task ConfirmUserEmailAddress_WhenConfirmationFails_ReturnsFailure()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.ConfirmEmailAsync(user, "token-123"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "bad token" }));

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = TestServiceProviderFactory.Create(nameof(this.ConfirmUserEmailAddress_WhenConfirmationFails_ReturnsFailure), userManager.Object, signInManager.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.ConfirmUserEmailAddressCommand("alice", "token-123"));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(SimpleResults.ResultStatus.Failure);
    }

    [Fact]
    public async Task SendWelcomeEmail_UsesResolvedMediatorAndSendsEmail()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.RemovePasswordAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(instance => instance.AddPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = TestServiceProviderFactory.Create(nameof(this.SendWelcomeEmail_UsesResolvedMediatorAndSendsEmail), userManager.Object, signInManager.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.SendWelcomeEmailCommand("alice"));
        var messagingClient = provider.GetRequiredService<IMessagingServiceClient>().ShouldBeOfType<TestMessagingServiceClient>();

        result.IsSuccess.ShouldBeTrue();
        userManager.Verify(instance => instance.FindByNameAsync("alice"), Times.Once);
        userManager.Verify(instance => instance.RemovePasswordAsync(user), Times.Once);
        userManager.Verify(instance => instance.AddPasswordAsync(user, It.IsAny<string>()), Times.Once);
        messagingClient.LastEmailRequest.ShouldNotBeNull();
        messagingClient.LastEmailRequest.Subject.ShouldBe("Welcome to Transaction Processing");
        messagingClient.LastEmailRequest.ToAddresses.ShouldContain("alice@example.com");
    }

    [Fact]
    public async Task SendWelcomeEmail_WhenRemovePasswordFails_ReturnsFailure()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.RemovePasswordAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "cannot remove" }));

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = TestServiceProviderFactory.Create(nameof(this.SendWelcomeEmail_WhenRemovePasswordFails_ReturnsFailure), userManager.Object, signInManager.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.SendWelcomeEmailCommand("alice"));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(SimpleResults.ResultStatus.Failure);
        userManager.Verify(instance => instance.AddPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendWelcomeEmail_WhenAddPasswordFails_ReturnsFailure()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.RemovePasswordAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(instance => instance.AddPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "cannot add" }));

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = TestServiceProviderFactory.Create(nameof(this.SendWelcomeEmail_WhenAddPasswordFails_ReturnsFailure), userManager.Object, signInManager.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.SendWelcomeEmailCommand("alice"));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(SimpleResults.ResultStatus.Failure);
    }

    [Fact]
    public async Task ProcessPasswordResetRequest_UsesResolvedMediatorAndSendsEmail()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("token+/=");

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = TestServiceProviderFactory.Create(nameof(this.ProcessPasswordResetRequest_UsesResolvedMediatorAndSendsEmail), userManager.Object, signInManager.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.ProcessPasswordResetRequestCommand("alice", "alice", "test-client-id"));
        var messagingClient = provider.GetRequiredService<IMessagingServiceClient>().ShouldBeOfType<TestMessagingServiceClient>();

        result.IsSuccess.ShouldBeTrue();
        userManager.Verify(instance => instance.FindByNameAsync("alice"), Times.Once);
        userManager.Verify(instance => instance.GeneratePasswordResetTokenAsync(user), Times.Once);
        messagingClient.LastEmailRequest.ShouldNotBeNull();
        messagingClient.LastEmailRequest.Subject.ShouldBe("Password Reset Requested");
        messagingClient.LastEmailRequest.ToAddresses.ShouldContain("alice@example.com");
        messagingClient.LastEmailRequest.Body.ShouldContain("/Account/ResetPassword?userName=alice&resetToken=token%2B%2F%3D&clientId=test-client-id");
    }

    [Fact]
    public async Task ProcessPasswordResetRequest_WhenUserMissing_ReturnsSuccessAndDoesNotSendEmail()
    {
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync((ApplicationUser?)null);

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = TestServiceProviderFactory.Create(nameof(this.ProcessPasswordResetRequest_WhenUserMissing_ReturnsSuccessAndDoesNotSendEmail), userManager.Object, signInManager.Object);
        var mediator = provider.GetRequiredService<IMediator>();
        var messagingClient = provider.GetRequiredService<IMessagingServiceClient>().ShouldBeOfType<TestMessagingServiceClient>();

        var result = await mediator.Send(new SecurityServiceCommands.ProcessPasswordResetRequestCommand("alice", "alice", "test-client-id"));

        result.IsSuccess.ShouldBeTrue();
        userManager.Verify(instance => instance.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        messagingClient.LastEmailRequest.ShouldBeNull();
    }

    [Fact]
    public async Task ProcessPasswordResetConfirmation_UsesResolvedMediatorAndReturnsClientUri()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.ResetPasswordAsync(user, "token-123", "NewPassword1!"))
            .ReturnsAsync(IdentityResult.Success);

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = TestServiceProviderFactory.Create(nameof(this.ProcessPasswordResetConfirmation_UsesResolvedMediatorAndReturnsClientUri), userManager.Object, signInManager.Object);
        await SeedClientAsync(provider, "test-client-id", "http://localhost/app");
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.ProcessPasswordResetConfirmationCommand("alice", "token-123", "NewPassword1!", "test-client-id"));

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBe("http://localhost/app");
        userManager.Verify(instance => instance.FindByNameAsync("alice"), Times.Once);
        userManager.Verify(instance => instance.ResetPasswordAsync(user, "token-123", "NewPassword1!"), Times.Once);
    }

    [Fact]
    public async Task ProcessPasswordResetConfirmation_WhenUserMissing_ReturnsNotFound()
    {
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync((ApplicationUser?)null);

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = TestServiceProviderFactory.Create(nameof(this.ProcessPasswordResetConfirmation_WhenUserMissing_ReturnsNotFound), userManager.Object, signInManager.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.ProcessPasswordResetConfirmationCommand("alice", "token-123", "NewPassword1!", "test-client-id"));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(SimpleResults.ResultStatus.NotFound);
        userManager.Verify(instance => instance.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPasswordResetConfirmation_WhenResetFails_ReturnsFailure()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.ResetPasswordAsync(user, "token-123", "NewPassword1!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "bad token" }));

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = TestServiceProviderFactory.Create(nameof(this.ProcessPasswordResetConfirmation_WhenResetFails_ReturnsFailure), userManager.Object, signInManager.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.ProcessPasswordResetConfirmationCommand("alice", "token-123", "NewPassword1!", "test-client-id"));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(SimpleResults.ResultStatus.Failure);
    }

    [Fact]
    public async Task ProcessPasswordResetConfirmation_WhenClientMissing_ReturnsInvalid()
    {
        var user = new ApplicationUser
        {
            UserName = "alice",
            Email = "alice@example.com"
        };
        var userManager = IdentityMocks.CreateUserManager();
        userManager.Setup(instance => instance.FindByNameAsync("alice"))
            .ReturnsAsync(user);
        userManager.Setup(instance => instance.ResetPasswordAsync(user, "token-123", "NewPassword1!"))
            .ReturnsAsync(IdentityResult.Success);

        var signInManager = IdentityMocks.CreateSignInManager(userManager);
        using var provider = TestServiceProviderFactory.Create(nameof(this.ProcessPasswordResetConfirmation_WhenClientMissing_ReturnsInvalid), userManager.Object, signInManager.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.ProcessPasswordResetConfirmationCommand("alice", "token-123", "NewPassword1!", "missing-client-id"));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(SimpleResults.ResultStatus.Invalid);
    }

    private static async Task SeedClientAsync(ServiceProvider provider, string clientId, string clientUri)
    {
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();
        dbContext.ClientDefinitions.Add(new ClientDefinition
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            ClientName = clientId,
            ClientUri = clientUri,
            ClientType = "confidential"
        });
        await dbContext.SaveChangesAsync();
    }
}
