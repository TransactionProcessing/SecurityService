using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using SecurityService.BusinessLogic.Requests;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;
using SecurityService.Models;
using TransactionProcessing.UnionResults;

namespace SecurityService.UnitTests.RequestHandlers;

public class IdentityResourceRequestHandlerTests
{
    [Fact]
    public async Task IdentityResourceLifecycle_CreateGetAndList_Works()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.IdentityResourceLifecycle_CreateGetAndList_Works));
        var mediator = provider.GetRequiredService<IMediator>();

        var createResult = await mediator.Send(new SecurityServiceCommands.CreateIdentityResourceCommand(
            "profile",
            "Profile",
            "Profile identity scope",
            true,
            false,
            true,
            [OpenIddictConstants.Claims.GivenName, OpenIddictConstants.Claims.FamilyName]));

        createResult.IsSuccess().ShouldBeTrue();

        var getResult = await mediator.Send(new SecurityServiceQueries.GetIdentityResourceQuery("profile"));
        getResult.IsSuccess().ShouldBeTrue();
        getResult.Value.ShouldBeOfType<Success<IdentityResourceDetails>>().Value.Claims.ShouldContain(OpenIddictConstants.Claims.GivenName);

        var listResult = await mediator.Send(new SecurityServiceQueries.GetIdentityResourcesQuery());
        listResult.Value.ShouldBeOfType<Success<List<IdentityResourceDetails>>>().Value.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task CreateIdentityResource_WhenNameMissing_ReturnsFailure()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateIdentityResource_WhenNameMissing_ReturnsFailure));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.CreateIdentityResourceCommand(
            string.Empty,
            "Profile",
            "Profile identity scope",
            true,
            false,
            true,
            [OpenIddictConstants.Claims.GivenName]));

        result.IsSuccess().ShouldBeFalse();
        result.GetErrors().Single().ErrorType.ShouldBe(ErrorType.Failure);
    }

    [Fact]
    public async Task CreateIdentityResource_WhenDuplicate_ReturnsConflict()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateIdentityResource_WhenDuplicate_ReturnsConflict));
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new SecurityServiceCommands.CreateIdentityResourceCommand(
            "profile",
            "Profile",
            "Profile identity scope",
            true,
            false,
            true,
            [OpenIddictConstants.Claims.GivenName]);

        (await mediator.Send(command)).IsSuccess().ShouldBeTrue();
        var duplicateResult = await mediator.Send(command);

        duplicateResult.IsSuccess().ShouldBeFalse();
        duplicateResult.GetErrors().Single().ErrorType.ShouldBe(ErrorType.Conflict);
    }

    [Fact]
    public async Task GetIdentityResource_WhenMissing_ReturnsNotFound()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.GetIdentityResource_WhenMissing_ReturnsNotFound));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceQueries.GetIdentityResourceQuery("missing-resource"));

        result.IsSuccess().ShouldBeFalse();
        result.GetErrors().Single().ErrorType.ShouldBe(ErrorType.NotFound);
    }
}
