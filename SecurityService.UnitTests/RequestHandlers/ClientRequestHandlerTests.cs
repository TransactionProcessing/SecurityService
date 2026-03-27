using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic.Requests;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;
using SimpleResults;

namespace SecurityService.UnitTests.RequestHandlers;

public class ClientRequestHandlerTests
{
    [Fact]
    public async Task ClientLifecycle_CreateGetAndList_Works()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.ClientLifecycle_CreateGetAndList_Works));
        var mediator = provider.GetRequiredService<IMediator>();

        var createResult = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "test-client",
            "secret",
            "Test Client",
            "Client description",
            ["payments", OpenIddictConstants.Scopes.OpenId],
            [OpenIddictConstants.GrantTypes.AuthorizationCode, OpenIddictConstants.GrantTypes.RefreshToken],
            "https://client.example",
            ["https://client.example/signin-oidc"],
            ["https://client.example/signout-callback-oidc"],
            true,
            true));

        createResult.IsSuccess.ShouldBeTrue();
        
        var getResult = await mediator.Send(new SecurityServiceQueries.GetClientQuery("test-client"));
        getResult.IsSuccess.ShouldBeTrue();
        getResult.Data!.AllowedScopes.ShouldContain("payments");

        var listResult = await mediator.Send(new SecurityServiceQueries.GetClientsQuery());
        listResult.IsSuccess.ShouldBeTrue();
        listResult.Data.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task CreateClient_WhenGrantTypeIsUnsupported_ReturnsInvalid()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateClient_WhenGrantTypeIsUnsupported_ReturnsInvalid));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "test-client",
            "secret",
            "Test Client",
            null,
            [],
            ["custom-grant"],
            null,
            [],
            [],
            false,
            false));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.Message.ShouldContain("Unsupported grant types");
    }

    [Fact]
    public async Task CreateClient_WhenClientAlreadyExists_ReturnsConflict()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateClient_WhenClientAlreadyExists_ReturnsConflict));
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new SecurityServiceCommands.CreateClientCommand(
            "test-client",
            "secret",
            "Test Client",
            null,
            ["payments"],
            [OpenIddictConstants.GrantTypes.ClientCredentials],
            null,
            [],
            [],
            false,
            false);

        (await mediator.Send(command)).IsSuccess.ShouldBeTrue();

        var duplicateResult = await mediator.Send(command);

        duplicateResult.IsFailed.ShouldBeTrue();
        duplicateResult.Status.ShouldBe(ResultStatus.Conflict);
    }

    [Fact]
    public async Task CreateClient_WhenSecretIsMissing_PersistsPublicClientType()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateClient_WhenSecretIsMissing_PersistsPublicClientType));
        var mediator = provider.GetRequiredService<IMediator>();

        var createResult = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "public-client",
            null,
            "Public Client",
            null,
            [OpenIddictConstants.Scopes.OpenId],
            [OpenIddictConstants.GrantTypes.AuthorizationCode],
            null,
            ["https://client.example/signin-oidc"],
            [],
            false,
            false));

        createResult.IsSuccess.ShouldBeTrue();

        var getResult = await mediator.Send(new SecurityServiceQueries.GetClientQuery("public-client"));

        getResult.IsSuccess.ShouldBeTrue();
        getResult.Data!.ClientType.ShouldBe(OpenIddictConstants.ClientTypes.Public);
    }

    [Fact]
    public async Task GetClient_WhenMissing_ReturnsNotFound()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.GetClient_WhenMissing_ReturnsNotFound));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceQueries.GetClientQuery("missing-client"));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
}
