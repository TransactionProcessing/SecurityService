namespace SecurityService.UnitTests.RequestHandler;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
using SecurityService.BusinessLogic.Exceptions;
using Shared.Exceptions;
using Shared.Logger;
using Shouldly;
using Xunit;

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
    public async Task UserRequestHandler_CreateUserRequest_RequestIsHandled(String password)
    {
        CreateUserRequest request = CreateUserRequest.Create(Guid.Parse(TestData.UserId),
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

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);

        await this.RequestHandler.Handle(request, CancellationToken.None);

        Int32 userCount = await this.AuthenticationDbContext.Users.CountAsync();
        userCount.ShouldBe(1);
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserRequest_NewPasswordHashEmpty_RequestIsHandled(){
        CreateUserRequest request = TestData.CreateUserRequest;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole{
                                                                              Id = Guid.NewGuid().ToString(),
                                                                              Name = "TESTROLE1",
                                                                              NormalizedName = "TESTROLE1"
                                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.PasswordHasher.Setup(s => s.HashPassword(It.IsAny<IdentityUser>(), It.IsAny<String>())).Returns(String.Empty);

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);

        Should.Throw<IdentityResultException>(async () => {
                                                  await this.RequestHandler.Handle(request, CancellationToken.None);
                                              });
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserRequest_CreateFailed_RequestIsHandled(){
        CreateUserRequest request = TestData.CreateUserRequest;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole
                                                          {
                                                              Id = Guid.NewGuid().ToString(),
                                                              Name = "TESTROLE1",
                                                              NormalizedName = "TESTROLE1"
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        List<IdentityError> errors = new List<IdentityError>();
        errors.Add(new IdentityError());
        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        Should.Throw<IdentityResultException>(async () => {
                                                  await this.RequestHandler.Handle(request, CancellationToken.None);
                                              });
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserRequest_AddClaimsFailed_RequestIsHandled()
    {
        CreateUserRequest request = TestData.CreateUserRequest;

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
                               s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>()))
            .ReturnsAsync(IdentityResult.Success)
            .ReturnsAsync(IdentityResult.Success)
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        Should.Throw<IdentityResultException>(async () => {
                                                  await this.RequestHandler.Handle(request, CancellationToken.None);
                                              });
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserRequest_SendMailThrowsError_RequestIsHandled()
    {
        CreateUserRequest request = TestData.CreateUserRequest;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole
                                                          {
                                                              Id = Guid.NewGuid().ToString(),
                                                              Name = "TESTROLE1",
                                                              NormalizedName = "TESTROLE1"
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);

        this.SetupRequestHandlers.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(),
                                                                                It.IsAny<SendEmailRequest>(),
                                                                                It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        await this.RequestHandler.Handle(request, CancellationToken.None);
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserRequest_AddToRoleFailed_RequestIsHandled()
    {
        CreateUserRequest request = TestData.CreateUserRequest;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole
                                                          {
                                                              Id = "RoleId1",
                                                              Name = "TESTROLE1",
                                                              NormalizedName = "TESTROLE1"
                                                          });
        await this.AuthenticationDbContext.UserRoles.AddAsync(new IdentityUserRole<String>{
                                                                                              RoleId = "RoleId1",
                                                                                              UserId = request.UserId.ToString(),
                                                                                          });

        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);

        Should.Throw<IdentityResultException>(async () => {
                                                  await this.RequestHandler.Handle(request, CancellationToken.None);
                                              });
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserRequest_CreateFailedAndCleanUpFailed_RequestIsHandled()
    {
        CreateUserRequest request = TestData.CreateUserRequest;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole
                                                          {
                                                              Id = "RoleId1",
                                                              Name = "TESTROLE1",
                                                              NormalizedName = "TESTROLE1"
                                                          });
        await this.AuthenticationDbContext.UserRoles.AddAsync(new IdentityUserRole<String>
                                                              {
                                                                  RoleId = "RoleId1",
                                                                  UserId = request.UserId.ToString(),
                                                              });

        await this.AuthenticationDbContext.SaveChangesAsync();

        List<IdentityError> errors = new List<IdentityError>();
        errors.Add(new IdentityError());

        this.SetupRequestHandlers.UserValidator
            .SetupSequence(s =>
                               s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>()))
            .ReturnsAsync(IdentityResult.Success)
            .ReturnsAsync(IdentityResult.Success)
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()))
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));


        Should.Throw<IdentityResultException>(async () => {
                                                  await this.RequestHandler.Handle(request, CancellationToken.None);
                                              });
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserRequest_NullRoles_RequestIsHandled()
    {
        CreateUserRequest request = CreateUserRequest.Create(Guid.Parse(TestData.UserId),
                                                             TestData.GivenName,
                                                             TestData.MiddleName,
                                                             TestData.FamilyName,
                                                             TestData.UserName,
                                                             TestData.Password,
                                                             TestData.EmailAddress,
                                                             TestData.PhoneNumber,
                                                             TestData.Claims,
                                                             null);

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);

        await this.RequestHandler.Handle(request, CancellationToken.None);
        
        Int32 userCount = await this.AuthenticationDbContext.Users.CountAsync();
        userCount.ShouldBe(1);
    }

    [Fact]
    public async Task UserRequestHandler_CreateUserRequest_EmptyRoles_RequestIsHandled()
    {
        CreateUserRequest request = CreateUserRequest.Create(Guid.Parse(TestData.UserId),
                                                             TestData.GivenName,
                                                             TestData.MiddleName,
                                                             TestData.FamilyName,
                                                             TestData.UserName,
                                                             TestData.Password,
                                                             TestData.EmailAddress,
                                                             TestData.PhoneNumber,
                                                             TestData.Claims,
                                                             new List<String>());

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);

        await this.RequestHandler.Handle(request, CancellationToken.None);

        Int32 userCount = await this.AuthenticationDbContext.Users.CountAsync();
        userCount.ShouldBe(1);
    }

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

    [Fact]
    public async Task UserRequestHandler_ConfirmUserEmailAddressRequest_RequestIsHandled()
    {
        ConfirmUserEmailAddressRequest request = TestData.ConfirmUserEmailAddressRequest;

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
                                                          {
                                                              Id = TestData.CreateUserRequest.UserId.ToString(),
                                                              UserName = TestData.CreateUserRequest.UserName,
                                                              NormalizedUserName = TestData.CreateUserRequest.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);

        Boolean result = await this.RequestHandler.Handle(request, CancellationToken.None);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task UserRequestHandler_ConfirmUserEmailAddressRequest_UserNotFound_RequestIsHandled()
    {
        ConfirmUserEmailAddressRequest request = TestData.ConfirmUserEmailAddressRequest;
        
        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);

        Boolean result = await this.RequestHandler.Handle(request, CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task UserRequestHandler_ConfirmUserEmailAddressRequest_ConfirmFailed_RequestIsHandled()
    {
        ConfirmUserEmailAddressRequest request = TestData.ConfirmUserEmailAddressRequest;

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
                                                          {
                                                              Id = TestData.CreateUserRequest.UserId.ToString(),
                                                              UserName = TestData.CreateUserRequest.UserName,
                                                              NormalizedUserName = TestData.CreateUserRequest.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        List<IdentityError> errors = new List<IdentityError>();
        errors.Add(new IdentityError());
        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        Boolean result = await this.RequestHandler.Handle(request, CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task UserRequestHandler_ChangeUserPasswordRequest_RequestIsHandled()
    {
        ChangeUserPasswordRequest request = TestData.ChangeUserPasswordRequest;

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
                                                          {
                                                              Id = Guid.NewGuid().ToString(),
                                                              UserName = TestData.ChangeUserPasswordRequest.UserName,
                                                              NormalizedUserName = TestData.ChangeUserPasswordRequest.UserName.ToUpper(),
                                                              PasswordHash = TestData.ChangeUserPasswordRequest.CurrentPassword
        });
        await this.AuthenticationDbContext.SaveChangesAsync();
        
        await this.ConfigurationDbContext.Clients.AddAsync(new Client{
                                                                    ClientId = TestData.ChangeUserPasswordRequest.ClientId,
                                                                    ClientUri = "http://localhost"
                                                                });

        
        await this.ConfigurationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>(), It.IsAny<String>())).ReturnsAsync(IdentityResult.Success);

        (Boolean success, String clientUrl) result = await this.RequestHandler.Handle(request, CancellationToken.None);

        result.success.ShouldBeTrue();
        result.clientUrl.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task UserRequestHandler_ChangeUserPasswordRequest_UserNotFound_RequestIsHandled()
    {
        ChangeUserPasswordRequest request = TestData.ChangeUserPasswordRequest;
        
        (Boolean success, String clientUrl) result = await this.RequestHandler.Handle(request, CancellationToken.None);

        result.success.ShouldBeFalse();
        result.clientUrl.ShouldBeEmpty();
    }

    [Fact]
    public async Task UserRequestHandler_ChangeUserPasswordRequest_ClientNotFound_RequestIsHandled()
    {
        ChangeUserPasswordRequest request = TestData.ChangeUserPasswordRequest;

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
                                                          {
                                                              Id = Guid.NewGuid().ToString(),
                                                              UserName = TestData.ChangeUserPasswordRequest.UserName,
                                                              NormalizedUserName = TestData.ChangeUserPasswordRequest.UserName.ToUpper(),
                                                              PasswordHash = TestData.ChangeUserPasswordRequest.CurrentPassword
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();
        
        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>(), It.IsAny<String>())).ReturnsAsync(IdentityResult.Success);
        (Boolean success, String clientUrl) result = await this.RequestHandler.Handle(request, CancellationToken.None);

        result.success.ShouldBeFalse();
        result.clientUrl.ShouldBeEmpty();
    }

    [Fact]
    public async Task UserRequestHandler_ChangeUserPasswordRequest_PasswordChangeFailed_RequestIsHandled()
    {
        ChangeUserPasswordRequest request = TestData.ChangeUserPasswordRequest;

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
                                                          {
                                                              Id = Guid.NewGuid().ToString(),
                                                              UserName = TestData.ChangeUserPasswordRequest.UserName,
                                                              NormalizedUserName = TestData.ChangeUserPasswordRequest.UserName.ToUpper(),
                                                              PasswordHash = TestData.ChangeUserPasswordRequest.CurrentPassword
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        await this.ConfigurationDbContext.Clients.AddAsync(new Client
                                                           {
                                                               ClientId = TestData.ChangeUserPasswordRequest.ClientId,
                                                               ClientUri = "http://localhost"
                                                           });


        await this.ConfigurationDbContext.SaveChangesAsync();
        List<IdentityError> errors = new List<IdentityError>();
        errors.Add(new IdentityError());

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>(),
                                                                               It.IsAny<String>())).ReturnsAsync(IdentityResult.Failed(errors.ToArray()));
        (Boolean success, String clientUrl) result = await this.RequestHandler.Handle(request, CancellationToken.None);

        result.success.ShouldBeTrue();
        result.clientUrl.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetConfirmationRequest_RequestIsHandled()
    {
        ProcessPasswordResetConfirmationRequest request = TestData.ProcessPasswordResetConfirmationRequest;

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
                                                          {
                                                              Id = TestData.CreateUserRequest.UserId.ToString(),
                                                              UserName = TestData.CreateUserRequest.UserName,
                                                              NormalizedUserName = TestData.CreateUserRequest.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        await this.ConfigurationDbContext.Clients.AddAsync(new Client
                                                           {
                                                               ClientId = TestData.ChangeUserPasswordRequest.ClientId,
                                                               ClientUri = "http://localhost"
                                                           });


        await this.ConfigurationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>(), It.IsAny<String>())).ReturnsAsync(IdentityResult.Success);

        String result = await this.RequestHandler.Handle(request, CancellationToken.None);

        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetConfirmationRequest_UserNotFound_RequestIsHandled()
    {
        ProcessPasswordResetConfirmationRequest request = TestData.ProcessPasswordResetConfirmationRequest;
        
        String result = await this.RequestHandler.Handle(request, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetConfirmationRequest_ResetPasswordFailed_RequestIsHandled()
    {
        ProcessPasswordResetConfirmationRequest request = TestData.ProcessPasswordResetConfirmationRequest;

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
                                                          {
                                                              Id = TestData.CreateUserRequest.UserId.ToString(),
                                                              UserName = TestData.CreateUserRequest.UserName,
                                                              NormalizedUserName = TestData.CreateUserRequest.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        await this.ConfigurationDbContext.Clients.AddAsync(new Client
                                                           {
                                                               ClientId = TestData.ChangeUserPasswordRequest.ClientId,
                                                               ClientUri = "http://localhost"
                                                           });


        await this.ConfigurationDbContext.SaveChangesAsync();
        
        List<IdentityError> errors = new List<IdentityError>();
        errors.Add(new IdentityError());

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>(),
                                                                               It.IsAny<String>())).ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        String result = await this.RequestHandler.Handle(request, CancellationToken.None);

        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetConfirmationRequest_ClientNotFound_RequestIsHandled()
    {
        ProcessPasswordResetConfirmationRequest request = TestData.ProcessPasswordResetConfirmationRequest;

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
        {
            Id = TestData.CreateUserRequest.UserId.ToString(),
            UserName = TestData.CreateUserRequest.UserName,
            NormalizedUserName = TestData.CreateUserRequest.UserName.ToUpper()
        });
        await this.AuthenticationDbContext.SaveChangesAsync();
        
        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>(),
                                                                               It.IsAny<String>())).ReturnsAsync(IdentityResult.Success);

        String result = await this.RequestHandler.Handle(request, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetRequest_RequestIsHandled()
    {
        ProcessPasswordResetRequest request = TestData.ProcessPasswordResetRequest;

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
                                                          {
                                                              Id = TestData.CreateUserRequest.UserId.ToString(),
                                                              UserName = TestData.CreateUserRequest.UserName,
                                                              NormalizedUserName = TestData.CreateUserRequest.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();
        
        await this.RequestHandler.Handle(request, CancellationToken.None);
    }

    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetRequest_UserNotFound_RequestIsHandled()
    {
        ProcessPasswordResetRequest request = TestData.ProcessPasswordResetRequest;

        await this.RequestHandler.Handle(request, CancellationToken.None);
    }

    [Fact]
    public async Task UserRequestHandler_ProcessPasswordResetRequest_MessagingServiceThrowsException_RequestIsHandled()
    {
        ProcessPasswordResetRequest request = TestData.ProcessPasswordResetRequest;

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
                                                          {
                                                              Id = TestData.CreateUserRequest.UserId.ToString(),
                                                              UserName = TestData.CreateUserRequest.UserName,
                                                              NormalizedUserName = TestData.CreateUserRequest.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(),
                                                                                It.IsAny<SendEmailRequest>(),
                                                                                It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        await this.RequestHandler.Handle(request, CancellationToken.None);
    }

    [Fact]
    public async Task UserRequestHandler_SendWelcomeEmailRequest_RequestIsHandled()
    {
        SendWelcomeEmailRequest request = TestData.SendWelcomeEmailRequest;

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
                                                          {
                                                              Id = TestData.CreateUserRequest.UserId.ToString(),
                                                              UserName = TestData.CreateUserRequest.UserName,
                                                              NormalizedUserName = TestData.CreateUserRequest.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>(), It.IsAny<String>())).ReturnsAsync(IdentityResult.Success);

        await this.RequestHandler.Handle(request, CancellationToken.None);
    }

    [Fact]
    public async Task UserRequestHandler_SendWelcomeEmailRequest_MessagingServiceThrowsException_RequestIsHandled()
    {
        SendWelcomeEmailRequest request = TestData.SendWelcomeEmailRequest;

        await this.AuthenticationDbContext.Users.AddAsync(new IdentityUser
                                                          {
                                                              Id = TestData.CreateUserRequest.UserId.ToString(),
                                                              UserName = TestData.CreateUserRequest.UserName,
                                                              NormalizedUserName = TestData.CreateUserRequest.UserName.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync();

        this.SetupRequestHandlers.UserValidator.Setup(s => s.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);
        this.SetupRequestHandlers.PasswordValidator.Setup(p => p.ValidateAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>(), It.IsAny<String>())).ReturnsAsync(IdentityResult.Success);

        this.SetupRequestHandlers.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(),
                                                                                It.IsAny<SendEmailRequest>(),
                                                                                It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        await this.RequestHandler.Handle(request, CancellationToken.None);
    }
}
