using SimpleResults;

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
        SecurityServiceQueries.GetIdentityResourceQuery query = TestData.GetIdentityResourceQuery;

        await this.Context.IdentityResources.AddAsync(new IdentityResource()
                                                      {
                                                          Id = 1,
                                                          Name = query.IdentityResourceName
                                                      });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        var result= await this.RequestHandler.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Name.ShouldBe(query.IdentityResourceName);
    }

    [Fact]
    public async Task IdentityResourceRequestHandler_GetIdentityResourceRequest_RecordNotFound_RequestIsHandled()
    {
        SecurityServiceQueries.GetIdentityResourceQuery query = TestData.GetIdentityResourceQuery;

        var result = await this.RequestHandler.Handle(query, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task IdentityResourceRequestHandler_GetIdentityResourcesRequest_RequestIsHandled()
    {
        SecurityServiceQueries.GetIdentityResourcesQuery query = TestData.GetIdentityResourcesQuery;

        await this.Context.IdentityResources.AddAsync(new IdentityResource()
                                                      {
                                                          Id = 1,
                                                          Name = TestData.GetIdentityResourceQuery.IdentityResourceName
                                                      });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        var result = await this.RequestHandler.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task IdentityResourceRequestHandler_GetIdentityResourcesRequest_NoRecordsFound_RequestIsHandled()
    {
        SecurityServiceQueries.GetIdentityResourcesQuery query = TestData.GetIdentityResourcesQuery;

        var result = await this.RequestHandler.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldBeEmpty();
    }
}