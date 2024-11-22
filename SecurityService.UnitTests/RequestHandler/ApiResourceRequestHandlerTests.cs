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

public class ApiResourceRequestHandlerTests{

    private readonly ApiResourceRequestHandler RequestHandler;

    private readonly ConfigurationDbContext Context;

    public ApiResourceRequestHandlerTests(){
        SetupRequestHandlers setupRequestHandlers = new SetupRequestHandlers();
        this.Context = SetupRequestHandlers.GetConfigurationDbContext();
        this.RequestHandler = setupRequestHandlers.SetupApiResourceRequestHandler(this.Context);
    }

    [Fact]
    public async Task ApiResourceRequestHandler_CreateApiResourceRequest_RequestIsHandled(){
        SecurityServiceCommands.CreateApiResourceCommand command = TestData.CreateApiResourceCommand;

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        Int32 resourceCount = await this.Context.ApiResources.CountAsync();
        resourceCount.ShouldBe(1);
    }

    [Fact]
    public async Task ApiResourceRequestHandler_CreateApiResourceRequest_NoScopes_RequestIsHandled()
    {
        SecurityServiceCommands.CreateApiResourceCommand command = TestData.CreateApiResourceCommand;
        command = command with { Scopes = new List<String>() };

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        Int32 resourceCount = await this.Context.ApiResources.CountAsync();
        resourceCount.ShouldBe(1);
    }

    [Fact]
    public async Task ApiResourceRequestHandler_CreateApiResourceRequest_NullScopes_RequestIsHandled(){
        SecurityServiceCommands.CreateApiResourceCommand command = TestData.CreateApiResourceCommand;
        command = command with { Scopes = null };

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        Int32 resourceCount = await this.Context.ApiResources.CountAsync();
        resourceCount.ShouldBe(1);
    }

    [Fact]
    public async Task ApiResourceRequestHandler_GetApiResourceRequest_RequestIsHandled()
    {
        SecurityServiceQueries.GetApiResourceQuery query = TestData.GetApiResourceQuery;

        await this.Context.ApiResources.AddAsync(new ApiResource{
                                                                    Id = 1,
                                                                    Name = TestData.GetApiResourceQuery.Name
                                                                });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        var result = await this.RequestHandler.Handle(query, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Name.ShouldBe(query.Name);
    }

    [Fact]
    public async Task ApiResourceRequestHandler_GetApiResourceRequest_RecordNotFound_RequestIsHandled()
    {
        SecurityServiceQueries.GetApiResourceQuery query = TestData.GetApiResourceQuery;

        var result = await this.RequestHandler.Handle(query, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task ApiResourceRequestHandler_GetApiResourcesRequest_RequestIsHandled()
    {
        SecurityServiceQueries.GetApiResourcesQuery query = TestData.GetApiResourcesQuery;

        await this.Context.ApiResources.AddAsync(new ApiResource
                                                 {
                                                     Id = 1,
                                                     Name = TestData.GetApiResourceQuery.Name
                                                 });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        var result = await this.RequestHandler.Handle(query, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task ApiResourceRequestHandler_GetApiResourcesRequest_NoRecordsFound_RequestIsHandled()
    {
        SecurityServiceQueries.GetApiResourcesQuery query = TestData.GetApiResourcesQuery;

        var result = await this.RequestHandler.Handle(query, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldBeEmpty();
    }
}