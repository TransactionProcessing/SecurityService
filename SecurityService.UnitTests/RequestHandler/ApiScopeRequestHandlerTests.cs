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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class ApiScopeRequestHandlerTests
{

    private readonly ApiScopeRequestHandler RequestHandler;

    private readonly ConfigurationDbContext Context;

    public ApiScopeRequestHandlerTests(){
        SetupRequestHandlers setupRequestHandlers = new SetupRequestHandlers();
        this.Context = SetupRequestHandlers.GetConfigurationDbContext();
        this.RequestHandler = setupRequestHandlers.SetupApiScopeRequestHandler(this.Context);
    }

    [Fact]
    public async Task ApiScopeRequestHandler_CreateApiScopeRequest_RequestIsHandled()
    {
        SecurityServiceCommands.CreateApiScopeCommand command = TestData.CreateApiScopeCommand;

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        Int32 scopeCount = await this.Context.ApiScopes.CountAsync();
        scopeCount.ShouldBe(1);
    }

    [Fact]
    public async Task ApiScopeRequestHandler_GetApiScopeRequest_RequestIsHandled()
    {
        SecurityServiceQueries.GetApiScopeQuery query = TestData.GetApiScopeQuery;

        await this.Context.ApiScopes.AddAsync(new ApiScope()
                                              {
                                                  Id = 1,
                                                  Name = TestData.GetApiScopeQuery.Name
                                              });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        var result = await this.RequestHandler.Handle(query, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Name.ShouldBe(query.Name);
    }

    [Fact]
    public async Task ApiScopeRequestHandler_GetApiScopeRequest_RecordNotFound_RequestIsHandled()
    {
        SecurityServiceQueries.GetApiScopeQuery query = TestData.GetApiScopeQuery;

        var result = await this.RequestHandler.Handle(query, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task ApiScopeRequestHandler_GetApiScopesRequest_RequestIsHandled()
    {
        SecurityServiceQueries.GetApiScopesQuery query = TestData.GetApiScopesQuery;

        await this.Context.ApiScopes.AddAsync(new ApiScope()
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
    public async Task ApiScopeRequestHandler_GetApiScopesRequest_NoRecordsFound_RequestIsHandled()
    {
        SecurityServiceQueries.GetApiScopesQuery query = TestData.GetApiScopesQuery;

        await this.Context.SaveChangesAsync(CancellationToken.None);

        var result = await this.RequestHandler.Handle(query, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldBeEmpty();
    }
}