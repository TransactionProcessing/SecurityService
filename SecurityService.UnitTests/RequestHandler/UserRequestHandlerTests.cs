using SimpleResults;

namespace SecurityService.UnitTests.RequestHandler;

using BusinessLogic.RequestHandlers;
using BusinessLogic.Requests;
using Castle.Components.DictionaryAdapter;
using Database.DbContexts;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using MessagingService.DataTransferObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using Moq;
using SecurityService.BusinessLogic;
using SecurityService.BusinessLogic.Exceptions;
using Shared.Exceptions;
using Shared.Logger;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static SecurityService.BusinessLogic.Requests.SecurityServiceCommands;

public class UserRequestHandlerTests{
    private readonly ConfigurationDbContext ConfigurationDbContext;
    private readonly AuthenticationDbContext AuthenticationDbContext;
    public UserRequestHandler RequestHandler;

    private SetupRequestHandlers SetupRequestHandlers;

    public UserRequestHandlerTests(){
        Logger.Initialise(NullLogger.Instance);

        this.SetupRequestHandlers = new SetupRequestHandlers();
        this.ConfigurationDbContext = SetupRequestHandlers.GetConfigurationDbContext();
        this.AuthenticationDbContext = SetupRequestHandlers.GetAuthenticationDbContext();
        this.RequestHandler = this.SetupRequestHandlers.SetUserRequestHandler(this.ConfigurationDbContext,this.AuthenticationDbContext);
    }

    [Theory]
    [InlineData("password")]
    [InlineData(null)]
    [InlineData("")]
    public async Task UserRequestHandler_CreateUserCommand_RequestIsHandled(String password)
    {
        SecurityServiceCommands.CreateUserCommand command = new(Guid.Parse(TestData.UserId),
                                                             TestData.GivenName,
                                                             TestData.MiddleName,
                                                             TestData.FamilyName,
                                                             TestData.UserName,
                                                             password,
                                                             TestData.EmailAddress,
                                                             TestData.PhoneNumber,
                                                             TestData.Claims,
                                                             TestData.Roles);


        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole{
                                                                              Id = Guid.NewGuid().ToString(),
                                                                              Name = "TESTROLE1",
                                                                              NormalizedName = "TESTROLE1"});
        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

        Result result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        Int32 userCount = await this.AuthenticationDbContext.Users.CountAsync();
        userCount.ShouldBe(1);
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserCommand_NewPasswordHashEmpty_RequestIsHandled() {
        SecurityServiceCommands.CreateUserCommand command = TestData.CreateUserCommand;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole{
                                                                              Id = Guid.NewGuid().ToString(),
                                                                              Name = "TESTROLE1",
                                                                              NormalizedName = "TESTROLE1"
                                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.PasswordHasher.Setup(s => s.HashPassword(It.IsAny<ApplicationUser>(), It.IsAny<String>())).Returns(String.Empty);

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }
    
    [Fact]
    public async Task UserRequestHandler_CreateUserCommand_CreateFailed_RequestIsHandled(){
        SecurityServiceCommands.CreateUserCommand command = TestData.CreateUserCommand;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole
                                                          {
                                                              Id = Guid.NewGuid().ToString(),
                                                              Name = "TESTROLE1",
                                                              NormalizedName = "TESTROLE1"
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        List<IdentityError> errors = new List<IdentityError>();
        errors.Add(new IdentityError());
        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserCommand_AddClaimsFailed_RequestIsHandled()
    {
        SecurityServiceCommands.CreateUserCommand command = TestData.CreateUserCommand;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole
                                                          {
                                                              Id = Guid.NewGuid().ToString(),
                                                              Name = "TESTROLE1",
                                                              NormalizedName = "TESTROLE1"
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        List<IdentityError> errors = new List<IdentityError>();
        errors.Add(new IdentityError());
        
        this.SetupRequestHandlers.UserValidator
            .SetupSequence(s =>
                               s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success)
            .ReturnsAsync(IdentityResult.Success)
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserCommand_SendMailThrowsError_RequestIsHandled()
    {
        SecurityServiceCommands.CreateUserCommand command = TestData.CreateUserCommand;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole
                                                          {
                                                              Id = Guid.NewGuid().ToString(),
                                                              Name = "TESTROLE1",
                                                              NormalizedName = "TESTROLE1"
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

        this.SetupRequestHandlers.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(),
                                                                                It.IsAny<SendEmailRequest>(),
                                                                                It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserCommand_AddToRoleFailed_RequestIsHandled()
    {
        SecurityServiceCommands.CreateUserCommand command = TestData.CreateUserCommand;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole
                                                          {
                                                              Id = "RoleId1",
                                                              Name = "TESTROLE1",
                                                              NormalizedName = "TESTROLE1"
                                                          });
        await this.AuthenticationDbContext.UserRoles.AddAsync(new IdentityUserRole<String>{
                                                                                              RoleId = "RoleId1",
                                                                                              UserId = command.UserId.ToString(),
                                                                                          });

        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserCommand_CreateFailedAndCleanUpFailed_RequestIsHandled()
    {
        SecurityServiceCommands.CreateUserCommand command = TestData.CreateUserCommand;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole
                                                          {
                                                              Id = "RoleId1",
                                                              Name = "TESTROLE1",
                                                              NormalizedName = "TESTROLE1"
                                                          });
        await this.AuthenticationDbContext.UserRoles.AddAsync(new IdentityUserRole<String>
                                                              {
                                                                  RoleId = "RoleId1",
                                                                  UserId = command.UserId.ToString(),
                                                              });

        await this.AuthenticationDbContext.SaveChangesAsync();

        List<IdentityError> errors = new List<IdentityError>();
        errors.Add(new IdentityError());

        this.SetupRequestHandlers.UserValidator
            .SetupSequence(s =>
                               s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success)
            .ReturnsAsync(IdentityResult.Success)
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()))
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));


        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    
    [Fact]
    public async Task UserRequestHandler_CreateUserCommand_NullRoles_RequestIsHandled()
    {
        SecurityServiceCommands.CreateUserCommand command = TestData.CreateUserCommand;
        command = command with { Roles = null };

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        Int32 userCount = await this.AuthenticationDbContext.Users.CountAsync();
        userCount.ShouldBe(1);
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserCommand_EmptyRoles_RequestIsHandled()
    {
        SecurityServiceCommands.CreateUserCommand command = TestData.CreateUserCommand;
        command = command with { Roles = new List<String>() };
        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        Int32 userCount = await this.AuthenticationDbContext.Users.CountAsync();
        userCount.ShouldBe(1);
    }
    /*
    [Fact]
    public async Task UserRequestHandler_GetUserRequest_RequestIsHandled()
    {
        GetUserRequest request = TestData.GetUserRequest;

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
                                                          {
                                                              Id = request.UserId.ToString()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        UserDetails model = await this.RequestHandler.Handle(request, CancellationToken.None);

        model.ShouldNotBeNull();
    }

    [Fact]
    public async Task UserRequestHandler_GetUserRequest_UserNotFound_RequestIsHandled()
    {
        GetUserRequest request = TestData.GetUserRequest;

        Should.Throw<NotFoundException>(async () => {
                                            await this.RequestHandler.Handle(request, CancellationToken.None);
                                        });
    }

    [Theory]
    [InlineData("00000001")]
    [InlineData(null)]
    [InlineData("")]
    public async Task UserRequestHandler_GetUsersRequest_RequestIsHandled(String userName)
    {
        GetUsersRequest request = GetUsersRequest.Create(userName);

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
                                                          {
                                                              Id = TestData.CreateUserRequest.UserId.ToString(),
                                                              UserName = TestData.CreateUserRequest.UserName,
                                                              NormalizedUserName = TestData.CreateUserRequest.UserName.ToUpper()
        });
        await this.AuthenticationDbContext.SaveChangesAsync();

        List<UserDetails> models = await this.RequestHandler.Handle(request, CancellationToken.None);

        models.ShouldHaveSingleItem();
    }
    */
    
    [Fact]
    public async Task UserRequestHandler_ConfirmUserEmailAddressCommand_RequestIsHandled()
    {
        SecurityServiceCommands.ConfirmUserEmailAddressCommand command = TestData.ConfirmUserEmailAddressCommand;

        await this.AuthenticationDbContext.Users.AddAsync(new ApplicationUser
        {
                                                              Id = TestData.CreateUserCommand.UserId.ToString(),
                                                              UserName = TestData.CreateUserCommand.UserName,
                                                              NormalizedUserName = TestData.CreateUserCommand.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

        Result result = await this.RequestHandler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task UserRequestHandler_ConfirmUserEmailAddressCommand_UserNotFound_RequestIsHandled()
    {
        SecurityServiceCommands.ConfirmUserEmailAddressCommand command = TestData.ConfirmUserEmailAddressCommand;

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

        Result result = await this.RequestHandler.Handle(command, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task UserRequestHandler_ConfirmUserEmailAddressCommand_ConfirmFailed_RequestIsHandled()
    {
        SecurityServiceCommands.ConfirmUserEmailAddressCommand command = TestData.ConfirmUserEmailAddressCommand;

        await this.AuthenticationDbContext.Users.AddAsync(new ApplicationUser
        {
                                                              Id = TestData.CreateUserCommand.UserId.ToString(),
                                                              UserName = TestData.CreateUserCommand.UserName,
                                                              NormalizedUserName = TestData.CreateUserCommand.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        List<IdentityError> errors = new List<IdentityError>();
        errors.Add(new IdentityError());
        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        Result result = await this.RequestHandler.Handle(command, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Failure);
    }
    
    [Fact]
    public async Task UserRequestHandler_ChangeUserPasswordCommand_RequestIsHandled()
    {
        SecurityServiceCommands.ChangeUserPasswordCommand command = TestData.ChangeUserPasswordCommand;

        await this.AuthenticationDbContext.Users.AddAsync(new ApplicationUser
        {
                                                              Id = Guid.NewGuid().ToString(),
                                                              UserName = TestData.ChangeUserPasswordCommand.UserName,
                                                              NormalizedUserName = TestData.ChangeUserPasswordCommand.UserName.ToUpper(),
                                                              PasswordHash = TestData.ChangeUserPasswordCommand.CurrentPassword
        });
        await this.AuthenticationDbContext.SaveChangesAsync();
        
        await this.ConfigurationDbContext.Clients.AddAsync(new Client{
                                                                    ClientId = TestData.ChangeUserPasswordCommand.ClientId,
                                                                    ClientUri = "http://localhost"
                                                                });

        
        await this.ConfigurationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>(), It.IsAny<String>())).ReturnsAsync(IdentityResult.Success);

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.IsSuccessful.ShouldBeTrue();
        result.Data.RedirectUri.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task UserRequestHandler_ChangeUserPasswordCommand_UserNotFound_RequestIsHandled()
    {
        ChangeUserPasswordCommand command = TestData.ChangeUserPasswordCommand;

        Result<ChangeUserPasswordResult> result = await this.RequestHandler.Handle(command, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task UserRequestHandler_ChangeUserPasswordCommand_ClientNotFound_RequestIsHandled()
    {
        ChangeUserPasswordCommand command = TestData.ChangeUserPasswordCommand;

        await this.AuthenticationDbContext.Users.AddAsync(new ApplicationUser
                                                          {
                                                              Id = Guid.NewGuid().ToString(),
                                                              UserName = TestData.ChangeUserPasswordCommand.UserName,
                                                              NormalizedUserName = TestData.ChangeUserPasswordCommand.UserName.ToUpper(),
                                                              PasswordHash = TestData.ChangeUserPasswordCommand.CurrentPassword
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();
        
        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>(), It.IsAny<String>())).ReturnsAsync(IdentityResult.Success);
        Result<ChangeUserPasswordResult> result = await this.RequestHandler.Handle(command, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Fact]
    public async Task UserRequestHandler_ChangeUserPasswordCommand_PasswordChangeFailed_RequestIsHandled()
    {
        ChangeUserPasswordCommand command = TestData.ChangeUserPasswordCommand;

        await this.AuthenticationDbContext.Users.AddAsync(new ApplicationUser
                                                          {
                                                              Id = Guid.NewGuid().ToString(),
                                                              UserName = TestData.ChangeUserPasswordCommand.UserName,
                                                              NormalizedUserName = TestData.ChangeUserPasswordCommand.UserName.ToUpper(),
                                                              PasswordHash = TestData.ChangeUserPasswordCommand.CurrentPassword
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        await this.ConfigurationDbContext.Clients.AddAsync(new Client
                                                           {
                                                               ClientId = TestData.ChangeUserPasswordCommand.ClientId,
                                                               ClientUri = "http://localhost"
                                                           });


        await this.ConfigurationDbContext.SaveChangesAsync();
        List<IdentityError> errors = new List<IdentityError>();
        errors.Add(new IdentityError());

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>(),
                                                                               It.IsAny<String>())).ReturnsAsync(IdentityResult.Failed(errors.ToArray()));
        Result<ChangeUserPasswordResult> result = await this.RequestHandler.Handle(command, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Failure);
    }
    
    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetConfirmationCommand_RequestIsHandled()
    {
        ProcessPasswordResetConfirmationCommand command = TestData.ProcessPasswordResetConfirmationCommand;

        await this.AuthenticationDbContext.Users.AddAsync(new ApplicationUser
                                                          {
                                                              Id = TestData.CreateUserCommand.UserId.ToString(),
                                                              UserName = TestData.CreateUserCommand.UserName,
                                                              NormalizedUserName = TestData.CreateUserCommand.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        await this.ConfigurationDbContext.Clients.AddAsync(new Client
                                                           {
                                                               ClientId = TestData.ChangeUserPasswordCommand.ClientId,
                                                               ClientUri = "http://localhost"
                                                           });


        await this.ConfigurationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>(), It.IsAny<String>())).ReturnsAsync(IdentityResult.Success);

        Result<String> result = await this.RequestHandler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetConfirmationCommand_UserNotFound_RequestIsHandled()
    {
        ProcessPasswordResetConfirmationCommand command = TestData.ProcessPasswordResetConfirmationCommand;

        Result<String> result = await this.RequestHandler.Handle(command, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetConfirmationCommand_ResetPasswordFailed_RequestIsHandled()
    {
        ProcessPasswordResetConfirmationCommand command = TestData.ProcessPasswordResetConfirmationCommand;

        await this.AuthenticationDbContext.Users.AddAsync(new ApplicationUser
                                                          {
                                                              Id = TestData.CreateUserCommand.UserId.ToString(),
                                                              UserName = TestData.CreateUserCommand.UserName,
                                                              NormalizedUserName = TestData.CreateUserCommand.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        await this.ConfigurationDbContext.Clients.AddAsync(new Client
                                                           {
                                                               ClientId = TestData.ChangeUserPasswordCommand.ClientId,
                                                               ClientUri = "http://localhost"
                                                           });


        await this.ConfigurationDbContext.SaveChangesAsync();
        
        List<IdentityError> errors = new List<IdentityError>();
        errors.Add(new IdentityError());

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>(),
                                                                               It.IsAny<String>())).ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        Result<String> result = await this.RequestHandler.Handle(command, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetConfirmationCommand_ClientNotFound_RequestIsHandled()
    {
        ProcessPasswordResetConfirmationCommand command = TestData.ProcessPasswordResetConfirmationCommand;

        await this.AuthenticationDbContext.Users.AddAsync(new ApplicationUser
        {
            Id = TestData.CreateUserCommand.UserId.ToString(),
            UserName = TestData.CreateUserCommand.UserName,
            NormalizedUserName = TestData.CreateUserCommand.UserName.ToUpper()
        });
        await this.AuthenticationDbContext.SaveChangesAsync();
        
        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>(),
                                                                               It.IsAny<String>())).ReturnsAsync(IdentityResult.Success);

        Result<String> result = await this.RequestHandler.Handle(command, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
    }
    
    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetRequestCommand_RequestIsHandled()
    {
        ProcessPasswordResetRequestCommand command = TestData.ProcessPasswordResetRequestCommand;

        await this.AuthenticationDbContext.Users.AddAsync(new ApplicationUser
                                                          {
                                                              Id = TestData.CreateUserCommand.UserId.ToString(),
                                                              UserName = TestData.CreateUserCommand.UserName,
                                                              NormalizedUserName = TestData.CreateUserCommand.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();


        this.SetupRequestHandlers.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(),
            It.IsAny<SendEmailRequest>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        Result result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetRequestCommand_UserNotFound_RequestIsHandled()
    {
        ProcessPasswordResetRequestCommand command = TestData.ProcessPasswordResetRequestCommand;

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetRequestCommand_MessagingServiceThrowsException_RequestIsHandled()
    {
        ProcessPasswordResetRequestCommand command = TestData.ProcessPasswordResetRequestCommand;

        await this.AuthenticationDbContext.Users.AddAsync(new ApplicationUser
                                                          {
                                                              Id = TestData.CreateUserCommand.UserId.ToString(),
                                                              UserName = TestData.CreateUserCommand.UserName,
                                                              NormalizedUserName = TestData.CreateUserCommand.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(),
                                                                                It.IsAny<SendEmailRequest>(),
                                                                                It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }
    
    [Fact]
    public async Task UserRequestHandler_SendWelcomeEmailRequest_RequestIsHandled()
    {
        SecurityServiceCommands.SendWelcomeEmailCommand command = TestData.SendWelcomeEmailCommand;

        await this.AuthenticationDbContext.Users.AddAsync(new ApplicationUser
        {
                                                              Id = TestData.CreateUserCommand.UserId.ToString(),
                                                              UserName = TestData.CreateUserCommand.UserName,
                                                              NormalizedUserName = TestData.CreateUserCommand.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>(), It.IsAny<String>())).ReturnsAsync(IdentityResult.Success);

        this.SetupRequestHandlers.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(),
            It.IsAny<SendEmailRequest>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task UserRequestHandler_SendWelcomeEmailRequest_MessagingServiceThrowsException_RequestIsHandled()
    {
        SecurityServiceCommands.SendWelcomeEmailCommand command = TestData.SendWelcomeEmailCommand;

        await this.AuthenticationDbContext.Users.AddAsync(new ApplicationUser
        {
                                                              Id = TestData.CreateUserCommand.UserId.ToString(),
                                                              UserName = TestData.CreateUserCommand.UserName,
                                                              NormalizedUserName = TestData.CreateUserCommand.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<ApplicationUser>>(), It.IsAny<ApplicationUser>(), It.IsAny<String>())).ReturnsAsync(IdentityResult.Success);

        this.SetupRequestHandlers.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(),
                                                                                It.IsAny<SendEmailRequest>(),
                                                                                It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }
}
