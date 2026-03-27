using SimpleResults;

namespace SecurityService.UnitTests.RequestHandler;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Exceptions;
using BusinessLogic.RequestHandlers;
using BusinessLogic.Requests;
using Database.DbContexts;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Models;
using Moq;
using Shared.Exceptions;
using Shouldly;
using Xunit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class RoleRequestHandlerTests
{
    private readonly RoleRequestHandler RequestHandler;
    private readonly ConfigurationDbContext ConfigurationDbContext;
    private readonly AuthenticationDbContext AuthenticationDbContext;

    private readonly SetupRequestHandlers SetupRequestHandlers;

    public RoleRequestHandlerTests(){
        this.SetupRequestHandlers = new SetupRequestHandlers();
        this.ConfigurationDbContext = SetupRequestHandlers.GetConfigurationDbContext();
        this.AuthenticationDbContext = SetupRequestHandlers.GetAuthenticationDbContext();
        this.RequestHandler = this.SetupRequestHandlers.SetupRoleRequestHandler(this.ConfigurationDbContext, this.AuthenticationDbContext);
    }

    [Fact]
    public async Task RoleRequestHandler_CreateRoleCommand_RequestIsHandled()
    {
        SecurityServiceCommands.CreateRoleCommand command = TestData.CreateRoleCommand;

        this.SetupRequestHandlers.RoleValidator.Setup(r => r.ValidateAsync(It.IsAny<RoleManager<IdentityRole>>(), It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        Int32 roleCount = await this.AuthenticationDbContext.Roles.CountAsync();
        roleCount.ShouldBe(1);
    }

    [Fact]
    public async Task RoleRequestHandler_CreateRoleCommand_RoleAlreadyExists_RequestIsHandled()
    {
        SecurityServiceCommands.CreateRoleCommand command = TestData.CreateRoleCommand;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole()
                                                          {
                                                              Id = TestData.CreateRoleCommand.RoleId.ToString(),
                                                              Name = TestData.CreateRoleCommand.Name,
                                                              NormalizedName = TestData.CreateRoleCommand.Name.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync(CancellationToken.None);

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();                                              
    }

    [Fact]
    public async Task RoleRequestHandler_CreateRoleCommand_RoleCreateFailed_RequestIsHandled()
    {
        SecurityServiceCommands.CreateRoleCommand command = TestData.CreateRoleCommand;

        List<IdentityError> errors = new List<IdentityError>();
        errors.Add(new IdentityError());
        this.SetupRequestHandlers.RoleValidator.Setup(r => r.ValidateAsync(It.IsAny<RoleManager<IdentityRole>>(), It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Failed(errors.ToArray()));
        
        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task RoleRequestHandler_GetRoleRequest_RequestIsHandled()
    {
        SecurityServiceQueries.GetRoleQuery query = TestData.GetRoleQuery;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole()
                                                          {
                                                              Id = TestData.GetRoleQuery.RoleId.ToString()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync(CancellationToken.None);

        var result = await this.RequestHandler.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();

        result.Data.RoleId.ShouldBe(query.RoleId);
    }

    [Fact]
    public async Task RoleRequestHandler_GetRoleRequest_RecordNotFound_RequestIsHandled()
    {
        SecurityServiceQueries.GetRoleQuery query = TestData.GetRoleQuery;

        var result = await this.RequestHandler.Handle(query, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task RoleRequestHandler_GetRolesRequest_RequestIsHandled()
    {
        SecurityServiceQueries.GetRolesQuery query = TestData.GetRolesQuery;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole()
                                                          {
                                                              Id = TestData.GetRoleQuery.RoleId.ToString()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync(CancellationToken.None);

        var result = await this.RequestHandler.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task RoleRequestHandler_GetRolesRequest_NoRecordsFound_RequestIsHandled(){
        SecurityServiceQueries.GetRolesQuery query = TestData.GetRolesQuery;
        
        var result = await this.RequestHandler.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldBeEmpty();
    }
}