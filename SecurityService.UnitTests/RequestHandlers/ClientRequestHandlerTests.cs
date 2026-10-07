using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using SecurityService.Database.DbContexts;
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
    public async Task CreateClient_PersistsGrantPermissionsAndPkceRequirement()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateClient_PersistsGrantPermissionsAndPkceRequirement));
        var mediator = provider.GetRequiredService<IMediator>();
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();

        var createResult = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "pkce-client",
            null,
            "PKCE Client",
            null,
            [OpenIddictConstants.Scopes.OpenId],
            [OpenIddictConstants.GrantTypes.AuthorizationCode, OpenIddictConstants.GrantTypes.RefreshToken],
            null,
            ["https://client.example/signin-oidc"],
            [],
            false,
            true));

        createResult.IsSuccess.ShouldBeTrue();

        object application = (await applicationManager.FindByClientIdAsync("pkce-client"))!;
        var permissions = (await applicationManager.GetPermissionsAsync(application)).ToHashSet(StringComparer.Ordinal);
        var requirements = (await applicationManager.GetRequirementsAsync(application)).ToHashSet(StringComparer.Ordinal);

        permissions.ShouldContain(OpenIddictConstants.Permissions.Endpoints.Authorization);
        permissions.ShouldContain(OpenIddictConstants.Permissions.Endpoints.Token);
        permissions.ShouldContain(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
        permissions.ShouldContain(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
        permissions.ShouldContain(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
        requirements.ShouldContain(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
    }

    [Fact]
    public async Task CreateClient_WithOfflineAccess_AddsRefreshTokenPermission()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateClient_WithOfflineAccess_AddsRefreshTokenPermission));
        var mediator = provider.GetRequiredService<IMediator>();
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();

        var createResult = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "offline-client",
            null,
            "Offline Client",
            null,
            [OpenIddictConstants.Scopes.OpenId, OpenIddictConstants.Scopes.OfflineAccess],
            [OpenIddictConstants.GrantTypes.AuthorizationCode],
            null,
            ["https://client.example/signin-oidc"],
            [],
            false,
            true));

        createResult.IsSuccess.ShouldBeTrue();

        object application = (await applicationManager.FindByClientIdAsync("offline-client"))!;
        var permissions = (await applicationManager.GetPermissionsAsync(application)).ToHashSet(StringComparer.Ordinal);

        permissions.ShouldContain(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
        permissions.ShouldContain(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
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
    public async Task CreateClient_WhenMetadataSaveFails_DoesNotLeaveOpenIddictApplication()
    {
        using var provider = TestServiceProviderFactory.Create(
            nameof(this.CreateClient_WhenMetadataSaveFails_DoesNotLeaveOpenIddictApplication),
            useSqlite: true,
            saveChangesInterceptor: new ThrowOnClientDefinitionSaveInterceptor());
        var mediator = provider.GetRequiredService<IMediator>();
        var dbContext = provider.GetRequiredService<SecurityServiceDbContext>();
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();

        await Should.ThrowAsync<InvalidOperationException>(() => mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "atomic-client",
            "secret",
            "Atomic Client",
            null,
            [OpenIddictConstants.Scopes.OpenId],
            [OpenIddictConstants.GrantTypes.ClientCredentials],
            null,
            [],
            [],
            false,
            false)));

        dbContext.ChangeTracker.Clear();
        using IServiceScope verificationScope = provider.CreateScope();
        var verificationApplicationManager = verificationScope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var verificationDbContext = verificationScope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();
        (await verificationApplicationManager.FindByClientIdAsync("atomic-client")).ShouldBeNull();
        (await verificationDbContext.ClientDefinitions.AnyAsync(client => client.ClientId == "atomic-client")).ShouldBeFalse();
    }

    [Fact]
    public async Task CreateClient_WhenOpenIddictApplicationAlreadyExists_CompletesMetadataRegistration()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateClient_WhenOpenIddictApplicationAlreadyExists_CompletesMetadataRegistration));
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();
        await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "retry-client",
            ClientSecret = "secret",
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit
        });

        var mediator = provider.GetRequiredService<IMediator>();
        var result = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "retry-client",
            "secret",
            "Retry Client",
            null,
            [OpenIddictConstants.Scopes.OpenId],
            [OpenIddictConstants.GrantTypes.ClientCredentials],
            null,
            [],
            [],
            false,
            false));

        result.IsSuccess.ShouldBeTrue();
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
