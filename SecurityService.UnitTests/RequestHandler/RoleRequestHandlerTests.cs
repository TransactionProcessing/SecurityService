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
    public async Task RoleRequestHandler_CreateRoleRequest_RequestIsHandled()
    {
        CreateRoleRequest request = TestData.CreateRoleRequest;

        this.SetupRequestHandlers.RoleValidator.Setup(r => r.ValidateAsync(It.IsAny<RoleManager<IdentityRole>>(), It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

        await this.RequestHandler.Handle(request, CancellationToken.None);

        Int32 roleCount = await this.AuthenticationDbContext.Roles.CountAsync();
        roleCount.ShouldBe(1);
    }

    [Fact]
    public async Task RoleRequestHandler_CreateRoleRequest_RoleAlreadyExists_RequestIsHandled()
    {
        CreateRoleRequest request = TestData.CreateRoleRequest;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole()
                                                          {
                                                              Id = TestData.CreateRoleRequest.RoleId.ToString(),
                                                              Name = TestData.CreateRoleRequest.Name,
                                                              NormalizedName = TestData.CreateRoleRequest.Name.ToUpper()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync(CancellationToken.None);

        Should.Throw<IdentityResultException>(async () =>
                                              {
                                                  await this.RequestHandler.Handle(request, CancellationToken.None);
                                              });
    }

    [Fact]
    public async Task RoleRequestHandler_CreateRoleRequest_RoleCreateFailed_RequestIsHandled()
    {
        CreateRoleRequest request = TestData.CreateRoleRequest;

        List<IdentityError> errors = new List<IdentityError>();
        errors.Add(new IdentityError());
        this.SetupRequestHandlers.RoleValidator.Setup(r => r.ValidateAsync(It.IsAny<RoleManager<IdentityRole>>(), It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Failed(errors.ToArray()));
        Should.Throw<IdentityResultException>(async () =>
                                              {
                                                  await this.RequestHandler.Handle(request, CancellationToken.None);
                                              });
    }

    [Fact]
    public async Task RoleRequestHandler_GetRoleRequest_RequestIsHandled()
    {
        GetRoleRequest request = TestData.GetRoleRequest;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole()
                                                          {
                                                              Id = TestData.GetRoleRequest.RoleId.ToString()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync(CancellationToken.None);

        RoleDetails model = await this.RequestHandler.Handle(request, CancellationToken.None);

        model.ShouldNotBeNull();
    }

    [Fact]
    public async Task RoleRequestHandler_GetRoleRequest_RecordNotFound_RequestIsHandled()
    {
        GetRoleRequest request = TestData.GetRoleRequest;

        Should.Throw<NotFoundException>(async () =>
                                        {
                                            await this.RequestHandler.Handle(request, CancellationToken.None);
                                        });
    }

    [Fact]
    public async Task RoleRequestHandler_GetIdentityResourcesRequest_RequestIsHandled()
    {
        GetRolesRequest request = TestData.GetRolesRequest;

        await this.AuthenticationDbContext.Roles.AddAsync(new IdentityRole()
                                                          {
                                                              Id = TestData.GetRoleRequest.RoleId.ToString()
                                                          });
        await this.AuthenticationDbContext.SaveChangesAsync(CancellationToken.None);

        List<RoleDetails> models = await this.RequestHandler.Handle(request, CancellationToken.None);

        models.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task RoleRequestHandler_GetIdentityResourcesRequest_NoRecordsFound_RequestIsHandled(){
        GetRolesRequest request = TestData.GetRolesRequest;

        List<RoleDetails> models = await this.RequestHandler.Handle(request, CancellationToken.None);

        models.ShouldBeEmpty();
    }
}