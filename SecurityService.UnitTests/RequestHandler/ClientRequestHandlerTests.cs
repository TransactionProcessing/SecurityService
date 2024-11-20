using SimpleResults;

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
    public async Task ClientRequestHandler_CreateClientCommand_RequestIsHandled()
    {
        SecurityServiceCommands.CreateClientCommand command = TestData.CreateClientCommand;

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        Int32 clientCount = await this.Context.Clients.CountAsync();
        clientCount.ShouldBe(1);
    }

    [Fact]
    public async Task ClientRequestHandler_CreateClientCommand_NullClientRedirectUris_RequestIsHandled()
    {
        SecurityServiceCommands.CreateClientCommand command = TestData.CreateClientCommand;

        command = command with { ClientRedirectUris = null };

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        Int32 clientCount = await this.Context.Clients.CountAsync();
        clientCount.ShouldBe(1);
    }

    [Fact]
    public async Task ClientRequestHandler_CreateClientCommand_EmptyClientRedirectUris_RequestIsHandled()
    {
        SecurityServiceCommands.CreateClientCommand command = TestData.CreateClientCommand;

        command = command with { ClientRedirectUris = new List<String>() };

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        
        Int32 clientCount = await this.Context.Clients.CountAsync();
        clientCount.ShouldBe(1);
    }

    [Fact]
    public async Task ClientRequestHandler_CreateClientRequest_EmptyClientPostLogoutRedirectUris_RequestIsHandled()
    {
        SecurityServiceCommands.CreateClientCommand command = TestData.CreateClientCommand;

        command = command with { ClientPostLogoutRedirectUris = new List<String>() };

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        Int32 clientCount = await this.Context.Clients.CountAsync();
        clientCount.ShouldBe(1);
    }

    
    [Fact]
    public async Task ClientRequestHandler_CreateClientRequest_InvalidGrant_RequestIsHandled(){
        SecurityServiceCommands.CreateClientCommand command = TestData.CreateClientCommand;

        command = command with { AllowedGrantTypes = new List<String>() {
            "invalid"
        } };

        Result result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }
    
    [Fact]
    public async Task ClientRequestHandler_CreateClientRequest_NullClientPostLogoutRedirectUris_RequestIsHandled()
    {
        SecurityServiceCommands.CreateClientCommand command = TestData.CreateClientCommand;
       
       command = command with { ClientPostLogoutRedirectUris = null };

       var result = await this.RequestHandler.Handle(command, CancellationToken.None);
       result.IsSuccess.ShouldBeTrue();
        Int32 clientCount = await this.Context.Clients.CountAsync();
        clientCount.ShouldBe(1);
    }

    [Fact]
    public async Task ClientRequestHandler_CreateClientRequest_HybridClient_RequestIsHandled()
    {
        SecurityServiceCommands.CreateClientCommand command = TestData.CreateHybridClientCommand;

        var result = await this.RequestHandler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

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