namespace SecurityService.UnitTests.RequestHandler;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.RequestHandlers;
using BusinessLogic.Requests;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using Duende.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using Shouldly;
using Xunit;

public class ClientRequestHandlerTests
{
    private readonly ClientRequestHandler RequestHandler;

    private readonly ConfigurationDbContext Context;

    public ClientRequestHandlerTests(){
        SetupRequestHandlers setupRequestHandlers = new SetupRequestHandlers();
        this.Context = SetupRequestHandlers.GetConfigurationDbContext();
        this.RequestHandler = setupRequestHandlers.SetupClientRequestHandler(this.Context);
    }

    [Fact]
    public async Task ClientRequestHandler_CreateClientRequest_RequestIsHandled()
    {
        CreateClientRequest request = TestData.CreateClientRequest;

        await this.RequestHandler.Handle(request, CancellationToken.None);

        Int32 clientCount = await this.Context.Clients.CountAsync();
        clientCount.ShouldBe(1);
    }

    [Fact]
    public async Task ClientRequestHandler_CreateClientRequest_NullClientRedirectUris_RequestIsHandled()
    {
        CreateClientRequest request = CreateClientRequest.Create(TestData.CreateClientRequest.ClientId,
                                                                 TestData.CreateClientRequest.Secret,
                                                                 TestData.CreateClientRequest.ClientName,
                                                                 TestData.CreateClientRequest.ClientDescription,
                                                                 TestData.CreateClientRequest.AllowedScopes,
                                                                 TestData.CreateClientRequest.AllowedGrantTypes,
                                                                 TestData.CreateClientRequest.ClientUri,
                                                                 null,
                                                                 TestData.CreateClientRequest.ClientPostLogoutRedirectUris,
                                                                 TestData.CreateClientRequest.RequireConsent,
                                                                 TestData.CreateClientRequest.AllowOfflineAccess);
                
        await this.RequestHandler.Handle(request, CancellationToken.None);

        Int32 clientCount = await this.Context.Clients.CountAsync();
        clientCount.ShouldBe(1);
    }

    [Fact]
    public async Task ClientRequestHandler_CreateClientRequest_EmptyClientRedirectUris_RequestIsHandled()
    {
        CreateClientRequest request = TestData.CreateClientRequest;
        request.ClientRedirectUris.Clear();
        await this.RequestHandler.Handle(request, CancellationToken.None);

        Int32 clientCount = await this.Context.Clients.CountAsync();
        clientCount.ShouldBe(1);
    }

    [Fact]
    public async Task ClientRequestHandler_CreateClientRequest_EmptyClientPostLogoutRedirectUris_RequestIsHandled()
    {
        CreateClientRequest request = TestData.CreateClientRequest;
        request.ClientPostLogoutRedirectUris.Clear();
        await this.RequestHandler.Handle(request, CancellationToken.None);

        Int32 clientCount = await this.Context.Clients.CountAsync();
        clientCount.ShouldBe(1);
    }

    [Fact]
    public async Task ClientRequestHandler_CreateClientRequest_InvalidGrant_RequestIsHandled(){
        CreateClientRequest request = CreateClientRequest.Create(TestData.CreateClientRequest.ClientId,
                                                                 TestData.CreateClientRequest.Secret,
                                                                 TestData.CreateClientRequest.ClientName,
                                                                 TestData.CreateClientRequest.ClientDescription,
                                                                 TestData.CreateClientRequest.AllowedScopes,
                                                                 new List<String>(),
                                                                 TestData.CreateClientRequest.ClientUri,
                                                                 TestData.CreateClientRequest.ClientRedirectUris,
                                                                 TestData.CreateClientRequest.ClientPostLogoutRedirectUris,
                                                                 TestData.CreateClientRequest.RequireConsent,
                                                                 TestData.CreateClientRequest.AllowOfflineAccess);
        request.AllowedGrantTypes.Add("invalid");

        Should.Throw<ArgumentException>(async () => {
                                            await this.RequestHandler.Handle(request, CancellationToken.None);
                                        });
    }

    [Fact]
    public async Task ClientRequestHandler_CreateClientRequest_NullClientPostLogoutRedirectUris_RequestIsHandled()
    {
        CreateClientRequest request = CreateClientRequest.Create(TestData.CreateClientRequest.ClientId,
                                                                 TestData.CreateClientRequest.Secret,
                                                                 TestData.CreateClientRequest.ClientName,
                                                                 TestData.CreateClientRequest.ClientDescription,
                                                                 TestData.CreateClientRequest.AllowedScopes,
                                                                 TestData.CreateClientRequest.AllowedGrantTypes,
                                                                 TestData.CreateClientRequest.ClientUri,
                                                                 TestData.CreateClientRequest.ClientRedirectUris,
                                                                 null,
                                                                 TestData.CreateClientRequest.RequireConsent,
                                                                 TestData.CreateClientRequest.AllowOfflineAccess);

        await this.RequestHandler.Handle(request, CancellationToken.None);

        Int32 clientCount = await this.Context.Clients.CountAsync();
        clientCount.ShouldBe(1);
    }

    [Fact]
    public async Task ClientRequestHandler_CreateClientRequest_HybridClient_RequestIsHandled()
    {
        CreateClientRequest request = TestData.CreateHybridClientRequest;

        await this.RequestHandler.Handle(request, CancellationToken.None);

        Int32 clientCount = await this.Context.Clients.CountAsync();
        clientCount.ShouldBe(1);
    }

    [Fact]
    public async Task ClientRequestHandler_GetClientRequest_RequestIsHandled()
    {
        GetClientRequest request = TestData.GetClientRequest;

        await this.Context.Clients.AddAsync(new Duende.IdentityServer.EntityFramework.Entities.Client()
                                            {
                                                ClientId = request.ClientId,
                                            });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        Client model = await this.RequestHandler.Handle(request, CancellationToken.None);

        model.ShouldNotBeNull();
        model.ClientId.ShouldBe(request.ClientId);
    }

    [Fact]
    public async Task ClientRequestHandler_GetClientRequest_RecordNotFound_RequestIsHandled()
    {
        GetClientRequest request = TestData.GetClientRequest;

        Should.Throw<NotFoundException>(async () => {
                                            await this.RequestHandler.Handle(request, CancellationToken.None);
                                        });
    }

    [Fact]
    public async Task ClientRequestHandler_GetClientsRequest_RequestIsHandled()
    {
        GetClientsRequest request = TestData.GetClientsRequest;

        await this.Context.Clients.AddAsync(new Duende.IdentityServer.EntityFramework.Entities.Client(){
                                                                                                           ClientId = TestData.GetClientRequest.ClientId,
                                                                                                       });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        List<Client> models = await this.RequestHandler.Handle(request, CancellationToken.None);

        models.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task ClientRequestHandler_GetClientsRequest_NoRecordsFound_RequestIsHandled()
    {
        GetClientsRequest request = TestData.GetClientsRequest;

        List<Client> models = await this.RequestHandler.Handle(request, CancellationToken.None);

        models.ShouldBeEmpty();
    }
}