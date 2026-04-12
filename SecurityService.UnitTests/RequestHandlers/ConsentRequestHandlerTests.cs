using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
    public async Task ConsentGetQuery_WhenNoOpenIddictServerRequest_ReturnsLocalRedirect()
    {
        var appManager = new Mock<IOpenIddictApplicationManager>();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(ConsentGetQuery_WhenNoOpenIddictServerRequest_ReturnsLocalRedirect));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var handler = new ConsentRequestHandler(appManager.Object, dbContext);
        var context = new DefaultHttpContext();

        var result = await handler.Handle(new OidcCommands.ConsentGetQuery(context, "/return"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var redirect = result.Data.ShouldBeOfType<ConsentGetLocalRedirectResult>();
        redirect.Url.ShouldBe("/return");
    }

    [Fact]
    public async Task ConsentPostCommand_WhenDenyButton_ReturnsRedirectWithDenied()
    {
        var appManager = new Mock<IOpenIddictApplicationManager>();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(ConsentPostCommand_WhenDenyButton_ReturnsRedirectWithDenied));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var handler = new ConsentRequestHandler(appManager.Object, dbContext);

        var result = await handler.Handle(
            new OidcCommands.ConsentPostCommand("/return", "deny", Array.Empty<string>()),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var redirect = result.Data.ShouldBeOfType<ConsentPostRedirectResult>();
        redirect.Url.ShouldContain("consent=denied");
    }

    [Fact]
    public async Task ConsentPostCommand_WhenNoScopesSelected_ReturnsPageWithError()
    {
        var appManager = new Mock<IOpenIddictApplicationManager>();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(ConsentPostCommand_WhenNoScopesSelected_ReturnsPageWithError));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var handler = new ConsentRequestHandler(appManager.Object, dbContext);

        var result = await handler.Handle(
            new OidcCommands.ConsentPostCommand("/return", "accept", Array.Empty<string>()),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var page = result.Data.ShouldBeOfType<ConsentPostPageResult>();
        page.ModelError.ShouldBe("Select at least one scope to continue.");
    }

    [Fact]
    public async Task ConsentPostCommand_WhenScopesSelected_ReturnsRedirectWithAccepted()
    {
        var appManager = new Mock<IOpenIddictApplicationManager>();
        using var serviceProvider = TestServiceProviderFactory.Create(nameof(ConsentPostCommand_WhenScopesSelected_ReturnsRedirectWithAccepted));
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        var handler = new ConsentRequestHandler(appManager.Object, dbContext);

        var result = await handler.Handle(
            new OidcCommands.ConsentPostCommand("/return", "accept", ["openid", "profile"]),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var redirect = result.Data.ShouldBeOfType<ConsentPostRedirectResult>();
        redirect.Url.ShouldContain("consent=accepted");
        redirect.Url.ShouldContain("granted_scope=openid");
        redirect.Url.ShouldContain("granted_scope=profile");
    }
}
