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
        CreateApiResourceRequest request = TestData.CreateApiResourceRequest;

        await this.RequestHandler.Handle(request, CancellationToken.None);

        Int32 resourceCount = await this.Context.ApiResources.CountAsync();
        resourceCount.ShouldBe(1);
    }

    [Fact]
    public async Task ApiResourceRequestHandler_CreateApiResourceRequest_NoScopes_RequestIsHandled()
    {
        CreateApiResourceRequest request = CreateApiResourceRequest.Create(TestData.CreateApiResourceRequest.Name,
                                                                           TestData.CreateApiResourceRequest.DisplayName,
                                                                           TestData.CreateApiResourceRequest.Description,
                                                                           TestData.CreateApiResourceRequest.Secret,
                                                                           new List<String>(),
                                                                           TestData.CreateApiResourceRequest.UserClaims);

        await this.RequestHandler.Handle(request, CancellationToken.None);

        Int32 resourceCount = await this.Context.ApiResources.CountAsync();
        resourceCount.ShouldBe(1);
    }

    [Fact]
    public async Task ApiResourceRequestHandler_CreateApiResourceRequest_NullScopes_RequestIsHandled(){
        CreateApiResourceRequest request = CreateApiResourceRequest.Create(TestData.CreateApiResourceRequest.Name,
                                                                           TestData.CreateApiResourceRequest.DisplayName,
                                                                           TestData.CreateApiResourceRequest.Description,
                                                                           TestData.CreateApiResourceRequest.Secret,
                                                                           null,
                                                                           TestData.CreateApiResourceRequest.UserClaims);

        await this.RequestHandler.Handle(request, CancellationToken.None);

        Int32 resourceCount = await this.Context.ApiResources.CountAsync();
        resourceCount.ShouldBe(1);
    }

    [Fact]
    public async Task ApiResourceRequestHandler_GetApiResourceRequest_RequestIsHandled()
    {
        GetApiResourceRequest request = TestData.GetApiResourceRequest;

        await this.Context.ApiResources.AddAsync(new ApiResource{
                                                                    Id = 1,
                                                                    Name = TestData.GetApiResourceRequest.Name
                                                                });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        Duende.IdentityServer.Models.ApiResource model = await this.RequestHandler.Handle(request, CancellationToken.None);

        model.ShouldNotBeNull();
        model.Name.ShouldBe(request.Name);
    }

    [Fact]
    public async Task ApiResourceRequestHandler_GetApiResourceRequest_RecordNotFound_RequestIsHandled()
    {
        GetApiResourceRequest request = TestData.GetApiResourceRequest;
            
        Should.Throw<NotFoundException>(async () => {
                                            await this.RequestHandler.Handle(request, CancellationToken.None);
                                        });
    }

    [Fact]
    public async Task ApiResourceRequestHandler_GetApiResourcesRequest_RequestIsHandled()
    {
        GetApiResourcesRequest request = TestData.GetApiResourcesRequest;

        await this.Context.ApiResources.AddAsync(new ApiResource
                                                 {
                                                     Id = 1,
                                                     Name = TestData.GetApiResourceRequest.Name
                                                 });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        List<Duende.IdentityServer.Models.ApiResource> models = await this.RequestHandler.Handle(request, CancellationToken.None);
            
        models.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task ApiResourceRequestHandler_GetApiResourcesRequest_NoRecordsFound_RequestIsHandled()
    {
        GetApiResourcesRequest request = TestData.GetApiResourcesRequest;
            
        List<Duende.IdentityServer.Models.ApiResource> models = await this.RequestHandler.Handle(request, CancellationToken.None);

        models.ShouldBeEmpty();
    }
}