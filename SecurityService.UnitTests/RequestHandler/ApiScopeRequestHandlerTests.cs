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
        CreateApiScopeRequest request = TestData.CreateApiScopeRequest;

        await this.RequestHandler.Handle(request, CancellationToken.None);

        Int32 scopeCount = await this.Context.ApiScopes.CountAsync();
        scopeCount.ShouldBe(1);
    }

    [Fact]
    public async Task ApiScopeRequestHandler_GetApiScopeRequest_RequestIsHandled()
    {
        GetApiScopeRequest request = TestData.GetApiScopeRequest;

        await this.Context.ApiScopes.AddAsync(new ApiScope()
                                              {
                                                  Id = 1,
                                                  Name = TestData.GetApiScopeRequest.Name
                                              });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        Duende.IdentityServer.Models.ApiScope model = await this.RequestHandler.Handle(request, CancellationToken.None);

        model.ShouldNotBeNull();
        model.Name.ShouldBe(request.Name);
    }

    [Fact]
    public async Task ApiScopeRequestHandler_GetApiScopeRequest_RecordNotFound_RequestIsHandled()
    {
        GetApiScopeRequest request = TestData.GetApiScopeRequest;

        Should.Throw<NotFoundException>(async () => {
                                            await this.RequestHandler.Handle(request, CancellationToken.None);
                                        });
    }

    [Fact]
    public async Task ApiScopeRequestHandler_GetApiScopesRequest_RequestIsHandled()
    {
        GetApiScopesRequest request = TestData.GetApiScopesRequest;

        await this.Context.ApiScopes.AddAsync(new ApiScope()
                                              {
                                                  Id = 1,
                                                  Name = TestData.GetApiResourceRequest.Name
                                              });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        List<Duende.IdentityServer.Models.ApiScope> models = await this.RequestHandler.Handle(request, CancellationToken.None);

        models.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task ApiScopeRequestHandler_GetApiScopesRequest_NoRecordsFound_RequestIsHandled()
    {
        GetApiScopesRequest request = TestData.GetApiScopesRequest;

        List<Duende.IdentityServer.Models.ApiScope> models = await this.RequestHandler.Handle(request, CancellationToken.None);

        models.ShouldBeEmpty();
    }
}