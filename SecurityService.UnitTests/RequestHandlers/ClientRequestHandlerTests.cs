using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic;
using SecurityService.BusinessLogic.RequestHandlers;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
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
    public async Task CreateClient_WhenClientIdIsBlank_ReturnsInvalidWithoutPersisting()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateClient_WhenClientIdIsBlank_ReturnsInvalidWithoutPersisting));
        var mediator = provider.GetRequiredService<IMediator>();
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();
        var dbContext = provider.GetRequiredService<SecurityServiceDbContext>();

        var result = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            " ",
            "secret",
            "Blank Client Id",
            null,
            [],
            [OpenIddictConstants.GrantTypes.ClientCredentials],
            null,
            [],
            [],
            false,
            false));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.Message.ShouldContain("Client id is required");
        (await applicationManager.FindByClientIdAsync(" ")).ShouldBeNull();
        (await dbContext.ClientDefinitions.AnyAsync()).ShouldBeFalse();
    }

    [Fact]
    public async Task CreateClient_WhenNoGrantTypesAreProvided_ReturnsInvalidWithoutPersisting()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateClient_WhenNoGrantTypesAreProvided_ReturnsInvalidWithoutPersisting));
        var mediator = provider.GetRequiredService<IMediator>();
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();
        var dbContext = provider.GetRequiredService<SecurityServiceDbContext>();

        var result = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "no-grants-client",
            "secret",
            "No Grants Client",
            null,
            [],
            [],
            null,
            [],
            [],
            false,
            false));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.Message.ShouldContain("At least one grant type is required");
        (await applicationManager.FindByClientIdAsync("no-grants-client")).ShouldBeNull();
        (await dbContext.ClientDefinitions.AnyAsync()).ShouldBeFalse();
    }

    [Fact]
    public async Task CreateClient_WithLegacyGrantType_IsRejectedUnlessClientIsConfigured()
    {
        using var provider = TestServiceProviderFactory.Create(
            nameof(this.CreateClient_WithLegacyGrantType_IsRejectedUnlessClientIsConfigured),
            configureOptions: options =>
            {
                options.OAuth.EnableLegacyGrantTypes = true;
                options.OAuth.LegacyGrantTypeClients[OpenIddictConstants.GrantTypes.Password] = ["legacy-client"];
            });
        var mediator = provider.GetRequiredService<IMediator>();

        var rejected = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "other-client",
            "secret",
            "Other Client",
            null,
            [],
            [OpenIddictConstants.GrantTypes.Password],
            null,
            [],
            [],
            false,
            false));

        rejected.IsFailed.ShouldBeTrue();
        rejected.Status.ShouldBe(ResultStatus.Invalid);

        var accepted = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "legacy-client",
            "secret",
            "Legacy Client",
            null,
            [],
            [OpenIddictConstants.GrantTypes.Password],
            null,
            [],
            [],
            false,
            false));

        accepted.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateClient_WithPasswordGrant_PersistsOnlyPasswordGrantPermission()
    {
        using var provider = TestServiceProviderFactory.Create(
            nameof(this.CreateClient_WithPasswordGrant_PersistsOnlyPasswordGrantPermission),
            configureOptions: options =>
            {
                options.OAuth.EnableLegacyGrantTypes = true;
                options.OAuth.LegacyGrantTypeClients[OpenIddictConstants.GrantTypes.Password] = ["legacy-client"];
            });
        var mediator = provider.GetRequiredService<IMediator>();
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();

        var result = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "legacy-client",
            "secret",
            "Legacy Client",
            null,
            [],
            [OpenIddictConstants.GrantTypes.Password],
            null,
            [],
            [],
            false,
            false));

        result.IsSuccess.ShouldBeTrue();

        object application = (await applicationManager.FindByClientIdAsync("legacy-client"))!;
        var permissions = (await applicationManager.GetPermissionsAsync(application)).ToHashSet(StringComparer.Ordinal);
        var expectedPermissions = new HashSet<string>(StringComparer.Ordinal)
        {
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.GrantTypes.Password
        };

        permissions.SetEquals(expectedPermissions).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateClient_WithInvalidRedirectUri_ReturnsInvalidWithoutPersisting()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateClient_WithInvalidRedirectUri_ReturnsInvalidWithoutPersisting));
        var mediator = provider.GetRequiredService<IMediator>();
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();
        var dbContext = provider.GetRequiredService<SecurityServiceDbContext>();

        var result = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "invalid-redirect-client",
            "secret",
            "Invalid Redirect Client",
            null,
            [],
            [OpenIddictConstants.GrantTypes.AuthorizationCode],
            null,
            ["http://client.example/signin-oidc"],
            [],
            false,
            false));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.Message.ShouldContain("ClientRedirectUris");
        (await applicationManager.FindByClientIdAsync("invalid-redirect-client")).ShouldBeNull();
        (await dbContext.ClientDefinitions.AnyAsync(client => client.ClientId == "invalid-redirect-client")).ShouldBeFalse();
    }

    [Fact]
    public async Task CreateClient_WithInvalidPostLogoutRedirectUri_ReturnsInvalidWithoutPersisting()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateClient_WithInvalidPostLogoutRedirectUri_ReturnsInvalidWithoutPersisting));
        var mediator = provider.GetRequiredService<IMediator>();
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();
        var dbContext = provider.GetRequiredService<SecurityServiceDbContext>();

        var result = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "invalid-post-logout-client",
            "secret",
            "Invalid Post Logout Client",
            null,
            [],
            [OpenIddictConstants.GrantTypes.AuthorizationCode],
            null,
            [],
            ["javascript:alert(1)"],
            false,
            false));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.Message.ShouldContain("ClientPostLogoutRedirectUris");
        (await applicationManager.FindByClientIdAsync("invalid-post-logout-client")).ShouldBeNull();
        (await dbContext.ClientDefinitions.AnyAsync(client => client.ClientId == "invalid-post-logout-client")).ShouldBeFalse();
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
    public async Task CreateClient_WhenMetadataSaveThrowsDbUpdateExceptionAndApplicationExists_ReturnsConflict()
    {
        using var provider = TestServiceProviderFactory.Create(
            nameof(this.CreateClient_WhenMetadataSaveThrowsDbUpdateExceptionAndApplicationExists_ReturnsConflict),
            useSqlite: true,
            saveChangesInterceptor: new ThrowOnClientDefinitionSaveInterceptor(new DbUpdateException("Injected duplicate.")));
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();
        await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "duplicate-after-save-failure",
            ClientSecret = "secret",
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit
        });
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "duplicate-after-save-failure",
            "secret",
            "Duplicate Client",
            null,
            [],
            [OpenIddictConstants.GrantTypes.ClientCredentials],
            null,
            [],
            [],
            false,
            false));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Conflict);
    }

    [Fact]
    public async Task CreateClient_WhenOpenIddictApplicationSaveThrowsDbUpdateExceptionWithoutDuplicate_RethrowsAndRollsBack()
    {
        using var provider = TestServiceProviderFactory.Create(
            nameof(this.CreateClient_WhenOpenIddictApplicationSaveThrowsDbUpdateExceptionWithoutDuplicate_RethrowsAndRollsBack),
            useSqlite: true,
            saveChangesInterceptor: new ThrowOnClientDefinitionSaveInterceptor(
                new DbUpdateException("Injected database failure."), throwOnOpenIddictApplicationSave: true));
        var mediator = provider.GetRequiredService<IMediator>();

        await Should.ThrowAsync<DbUpdateException>(() => mediator.Send(new SecurityServiceCommands.CreateClientCommand(
            "database-failure-client",
            "secret",
            "Database Failure Client",
            null,
            [],
            [OpenIddictConstants.GrantTypes.ClientCredentials],
            null,
            [],
            [],
            false,
            false)));

        using IServiceScope verificationScope = provider.CreateScope();
        var verificationApplicationManager = verificationScope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var verificationDbContext = verificationScope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();
        (await verificationApplicationManager.FindByClientIdAsync("database-failure-client")).ShouldBeNull();
        (await verificationDbContext.ClientDefinitions.AnyAsync(client => client.ClientId == "database-failure-client"))
            .ShouldBeFalse();
    }

    [Fact]
    public async Task PersistClient_WhenApplicationAndDefinitionAlreadyExist_ReturnsWithoutDuplicatingThem()
    {
        using var provider = TestServiceProviderFactory.Create(
            nameof(this.PersistClient_WhenApplicationAndDefinitionAlreadyExist_ReturnsWithoutDuplicatingThem),
            useSqlite: true);
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();
        await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "concurrent-client",
            ClientSecret = "secret",
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit
        });
        var dbContext = provider.GetRequiredService<SecurityServiceDbContext>();
        dbContext.ClientDefinitions.Add(new ClientDefinition
        {
            Id = Guid.NewGuid(),
            ClientId = "concurrent-client",
            ClientName = "Concurrent Client",
            AllowedScopesJson = "[]",
            AllowedGrantTypesJson = "[]",
            RedirectUrisJson = "[]",
            PostLogoutRedirectUrisJson = "[]",
            ClientType = OpenIddictConstants.ClientTypes.Confidential
        });
        await dbContext.SaveChangesAsync();
        var handler = new ClientRequestHandler(
            dbContext,
            applicationManager,
            provider.GetRequiredService<IOptions<ServiceOptions>>());
        var definition = await dbContext.ClientDefinitions.SingleAsync(client => client.ClientId == "concurrent-client");
        var persistClient = typeof(ClientRequestHandler).GetMethod("PersistClientAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

        await (Task)persistClient.Invoke(handler,
        [
            new OpenIddictApplicationDescriptor
            {
                ClientId = "concurrent-client",
                ClientSecret = "secret",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                ConsentType = OpenIddictConstants.ConsentTypes.Implicit
            },
            definition,
            "concurrent-client",
            CancellationToken.None
        ])!;

        (await applicationManager.FindByClientIdAsync("concurrent-client")).ShouldNotBeNull();
        (await dbContext.ClientDefinitions.CountAsync(client => client.ClientId == "concurrent-client"))
            .ShouldBe(1);
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
