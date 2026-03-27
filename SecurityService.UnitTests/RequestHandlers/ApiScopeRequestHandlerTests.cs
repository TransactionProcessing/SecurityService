using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SecurityService.BusinessLogic.Requests;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;
using SimpleResults;

namespace SecurityService.UnitTests.RequestHandlers;

public class ApiScopeRequestHandlerTests
{
    [Fact]
    public async Task ApiScopeLifecycle_CreateGetAndList_Works()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.ApiScopeLifecycle_CreateGetAndList_Works));
        var mediator = provider.GetRequiredService<IMediator>();

        var createResult = await mediator.Send(new SecurityServiceCommands.CreateApiScopeCommand("payments", "Payments", "Payments API scope"));
        createResult.IsSuccess.ShouldBeTrue();

        var getResult = await mediator.Send(new SecurityServiceQueries.GetApiScopeQuery("payments"));
        getResult.IsSuccess.ShouldBeTrue();
        getResult.Data!.DisplayName.ShouldBe("Payments");

        var listResult = await mediator.Send(new SecurityServiceQueries.GetApiScopesQuery());
        listResult.Data.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task CreateApiScope_WhenNameMissing_ReturnsInvalid()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateApiScope_WhenNameMissing_ReturnsInvalid));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.CreateApiScopeCommand(string.Empty, "Payments", "Payments API scope"));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Fact]
    public async Task CreateApiScope_WhenDuplicate_ReturnsConflict()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateApiScope_WhenDuplicate_ReturnsConflict));
        var mediator = provider.GetRequiredService<IMediator>();

        (await mediator.Send(new SecurityServiceCommands.CreateApiScopeCommand("payments", "Payments", "Payments API scope"))).IsSuccess.ShouldBeTrue();
        var duplicateResult = await mediator.Send(new SecurityServiceCommands.CreateApiScopeCommand("payments", "Payments", "Payments API scope"));

        duplicateResult.IsFailed.ShouldBeTrue();
        duplicateResult.Status.ShouldBe(ResultStatus.Conflict);
    }

    [Fact]
    public async Task GetApiScope_WhenMissing_ReturnsNotFound()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.GetApiScope_WhenMissing_ReturnsNotFound));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceQueries.GetApiScopeQuery("missing-scope"));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
}
