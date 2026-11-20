using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SecurityService.BusinessLogic.Requests;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;
using SecurityService.Models;
using TransactionProcessing.UnionResults;

namespace SecurityService.UnitTests.RequestHandlers;

public class ApiResourceRequestHandlerTests
{
    [Fact]
    public async Task ApiResourceLifecycle_CreateGetAndList_Works()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.ApiResourceLifecycle_CreateGetAndList_Works));
        var mediator = provider.GetRequiredService<IMediator>();

        await mediator.Send(new SecurityServiceCommands.CreateApiScopeCommand("payments", "Payments", "Payments API scope"));

        var createResult = await mediator.Send(new SecurityServiceCommands.CreateApiResourceCommand(
            "payments-api",
            "Payments API",
            "Payments resource",
            "resource-secret",
            ["payments"],
            ["merchant_id"]));

        createResult.IsSuccess().ShouldBeTrue();

        var getResult = await mediator.Send(new SecurityServiceQueries.GetApiResourceQuery("payments-api"));
        getResult.IsSuccess().ShouldBeTrue();
        getResult.Value.ShouldBeOfType<Success<ApiResourceDetails>>().Value.Scopes.ShouldContain("payments");

        var listResult = await mediator.Send(new SecurityServiceQueries.GetApiResourcesQuery());
        listResult.Value.ShouldBeOfType<Success<List<ApiResourceDetails>>>().Value.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task CreateApiResource_WhenNameMissing_ReturnsInvalid()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateApiResource_WhenNameMissing_ReturnsInvalid));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.CreateApiResourceCommand(
            string.Empty,
            "Payments API",
            "Payments resource",
            "resource-secret",
            ["payments"],
            ["merchant_id"]));

        result.IsSuccess().ShouldBeFalse();
        result.GetErrors().Single().ErrorType.ShouldBe(ErrorType.Validation);
    }

    [Fact]
    public async Task CreateApiResource_WhenDuplicate_ReturnsConflict()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateApiResource_WhenDuplicate_ReturnsConflict));
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new SecurityServiceCommands.CreateApiResourceCommand(
            "payments-api",
            "Payments API",
            "Payments resource",
            "resource-secret",
            ["payments"],
            ["merchant_id"]);

        (await mediator.Send(command)).IsSuccess().ShouldBeTrue();
        var duplicateResult = await mediator.Send(command);

        duplicateResult.IsSuccess().ShouldBeFalse();
        duplicateResult.GetErrors().Single().ErrorType.ShouldBe(ErrorType.Conflict);
    }

    [Fact]
    public async Task GetApiResource_WhenMissing_ReturnsNotFound()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.GetApiResource_WhenMissing_ReturnsNotFound));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceQueries.GetApiResourceQuery("missing-api-resource"));

        result.IsSuccess().ShouldBeFalse();
        result.GetErrors().Single().ErrorType.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateApiResource_WhenScopeDoesNotExist_StillSucceeds()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateApiResource_WhenScopeDoesNotExist_StillSucceeds));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.CreateApiResourceCommand(
            "payments-api",
            "Payments API",
            "Payments resource",
            "resource-secret",
            ["payments"],
            ["merchant_id"]));

        result.IsSuccess().ShouldBeTrue();

        var getResult = await mediator.Send(new SecurityServiceQueries.GetApiResourceQuery("payments-api"));
        getResult.IsSuccess().ShouldBeTrue();
        getResult.Value.ShouldBeOfType<Success<ApiResourceDetails>>().Value.Scopes.ShouldContain("payments");
    }
}
