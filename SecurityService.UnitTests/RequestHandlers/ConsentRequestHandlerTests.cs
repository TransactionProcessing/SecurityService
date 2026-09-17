using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Imposter.Abstractions;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic.Oidc;
using SecurityService.BusinessLogic.RequestHandlers;
using SecurityService.Database.DbContexts;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;

namespace SecurityService.UnitTests.RequestHandlers;

public class ConsentRequestHandlerTests
{
    [Fact]
    public async Task ConsentGetQuery_WhenTransactionIsInvalid_ReturnsControlledError()
    {
        var appManager = new IOpenIddictApplicationManagerImposter();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(ConsentGetQuery_WhenTransactionIsInvalid_ReturnsControlledError));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var handler = new ConsentRequestHandler(appManager.Instance(), dbContext, CreateProtector());
        var context = new DefaultHttpContext();

        var result = await handler.Handle(new OidcCommands.ConsentGetQuery(context, "/return"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeOfType<ConsentGetInvalidResult>();
    }

    [Fact]
    public async Task ConsentPostCommand_WhenDenyButton_ReturnsRedirectWithDenied()
    {
        var appManager = new IOpenIddictApplicationManagerImposter();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(ConsentPostCommand_WhenDenyButton_ReturnsRedirectWithDenied));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var handler = new ConsentRequestHandler(appManager.Instance(), dbContext, CreateProtector());

        var result = await handler.Handle(
            new OidcCommands.ConsentPostCommand(new DefaultHttpContext(), "invalid", "deny", Array.Empty<string>()),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeOfType<ConsentPostInvalidResult>();
    }

    [Fact]
    public async Task ConsentPostCommand_WhenNoScopesSelected_ReturnsPageWithError()
    {
        var appManager = new IOpenIddictApplicationManagerImposter();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(ConsentPostCommand_WhenNoScopesSelected_ReturnsPageWithError));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var handler = new ConsentRequestHandler(appManager.Instance(), dbContext, CreateProtector());

        var result = await handler.Handle(
            new OidcCommands.ConsentPostCommand(new DefaultHttpContext(), "invalid", "accept", Array.Empty<string>()),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeOfType<ConsentPostInvalidResult>();
    }

    [Fact]
    public async Task ConsentPostCommand_WhenScopesSelected_ReturnsRedirectWithAccepted()
    {
        var appManager = new IOpenIddictApplicationManagerImposter();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(ConsentPostCommand_WhenScopesSelected_ReturnsRedirectWithAccepted));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var handler = new ConsentRequestHandler(appManager.Instance(), dbContext, CreateProtector());

        var result = await handler.Handle(
            new OidcCommands.ConsentPostCommand(new DefaultHttpContext(), "invalid", "accept", ["openid", "profile"]),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeOfType<ConsentPostInvalidResult>();
    }

    private static ConsentTransactionProtector CreateProtector() => new(
        new EphemeralDataProtectionProvider().CreateProtector("consent"));
}
