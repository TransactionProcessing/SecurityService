namespace SecurityService.UnitTests.RequestHandler;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.RequestHandlers;
using BusinessLogic.Requests;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using Shouldly;
using Xunit;

public class IdentityResourceRequestHandlerTests
{
    private readonly IdentityResourceRequestHandler RequestHandler;

    private readonly ConfigurationDbContext Context;

    public IdentityResourceRequestHandlerTests(){
        SetupRequestHandlers setupRequestHandlers = new SetupRequestHandlers();
        this.Context = SetupRequestHandlers.GetConfigurationDbContext();
        this.RequestHandler = setupRequestHandlers.SetupIdentityResourceRequestHandler(this.Context);
    }

    [Fact]
    public async Task IdentityResourceRequestHandler_CreateIdentityResourceRequest_RequestIsHandled()
    {
        SecurityServiceCommands.CreateIdentityResourceCommand command = TestData.CreateIdentityResourceCommand;

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        Int32 clientCount = await this.Context.IdentityResources.CountAsync();
        clientCount.ShouldBe(1);
    }

    [Fact]
    public async Task IdentityResourceRequestHandler_GetIdentityResourceRequest_RequestIsHandled()
    {
        GetIdentityResourceRequest request = TestData.GetIdentityResourceRequest;

        await this.Context.IdentityResources.AddAsync(new IdentityResource()
                                                      {
                                                          Id = 1,
                                                          Name = request.IdentityResourceName
                                                      });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        Duende.IdentityServer.Models.IdentityResource model = await this.RequestHandler.Handle(request, CancellationToken.None);

        model.ShouldNotBeNull();
        model.Name.ShouldBe(request.IdentityResourceName);
    }

    [Fact]
    public async Task IdentityResourceRequestHandler_GetIdentityResourceRequest_RecordNotFound_RequestIsHandled()
    {
        GetIdentityResourceRequest request = TestData.GetIdentityResourceRequest;

        Should.Throw<NotFoundException>(async () =>
                                        {
                                            await this.RequestHandler.Handle(request, CancellationToken.None);
                                        });
    }

    [Fact]
    public async Task IdentityResourceRequestHandler_GetIdentityResourcesRequest_RequestIsHandled()
    {
        GetIdentityResourcesRequest request = TestData.GetIdentityResourcesRequest;

        await this.Context.IdentityResources.AddAsync(new IdentityResource()
                                                      {
                                                          Id = 1,
                                                          Name = TestData.GetIdentityResourceRequest.IdentityResourceName
                                                      });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        List<Duende.IdentityServer.Models.IdentityResource> models = await this.RequestHandler.Handle(request, CancellationToken.None);

        models.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task IdentityResourceRequestHandler_GetIdentityResourcesRequest_NoRecordsFound_RequestIsHandled()
    {
        GetIdentityResourcesRequest request = TestData.GetIdentityResourcesRequest;

        List<Duende.IdentityServer.Models.IdentityResource> models = await this.RequestHandler.Handle(request, CancellationToken.None);

        models.ShouldBeEmpty();
    }
}