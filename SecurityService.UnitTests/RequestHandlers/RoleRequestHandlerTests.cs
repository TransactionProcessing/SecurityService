using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SecurityService.BusinessLogic.Requests;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;
using SecurityService.Models;
using TransactionProcessing.UnionResults;

namespace SecurityService.UnitTests.RequestHandlers;

public class RoleRequestHandlerTests
{
    [Fact]
    public async Task RoleLifecycle_CreateGetAndList_Works()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.RoleLifecycle_CreateGetAndList_Works));
        var mediator = provider.GetRequiredService<IMediator>();

        var createResult = await mediator.Send(new SecurityServiceCommands.CreateRoleCommand("admin"));
        createResult.IsSuccess().ShouldBeTrue();
        
        var listResult = await mediator.Send(new SecurityServiceQueries.GetRolesQuery());
        listResult.Value.ShouldBeOfType<Success<List<RoleDetails>>>().Value.ShouldHaveSingleItem();

        var getResult = await mediator.Send(new SecurityServiceQueries.GetRoleQuery(listResult.Value.ShouldBeOfType<Success<List<RoleDetails>>>().Value.First().RoleId));
        getResult.IsSuccess().ShouldBeTrue();
        getResult.Value.ShouldBeOfType<Success<RoleDetails>>().Value.Name.ShouldBe("admin");
    }

    [Fact]
    public async Task CreateRole_WhenNameMissing_ReturnsInvalid()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateRole_WhenNameMissing_ReturnsInvalid));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.CreateRoleCommand(string.Empty));

        result.IsSuccess().ShouldBeFalse();
        result.GetErrors().Single().ErrorType.ShouldBe(ErrorType.Validation);
    }

    [Fact]
    public async Task CreateRole_WhenDuplicate_ReturnsConflict()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.CreateRole_WhenDuplicate_ReturnsConflict));
        var mediator = provider.GetRequiredService<IMediator>();

        (await mediator.Send(new SecurityServiceCommands.CreateRoleCommand("admin"))).IsSuccess().ShouldBeTrue();
        var duplicateResult = await mediator.Send(new SecurityServiceCommands.CreateRoleCommand("admin"));

        duplicateResult.IsSuccess().ShouldBeFalse();
        duplicateResult.GetErrors().Single().ErrorType.ShouldBe(ErrorType.Conflict);
    }

    [Fact]
    public async Task GetRole_WhenMissing_ReturnsNotFound()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.GetRole_WhenMissing_ReturnsNotFound));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceQueries.GetRoleQuery("missing-role-id"));

        result.IsSuccess().ShouldBeFalse();
        result.GetErrors().Single().ErrorType.ShouldBe(ErrorType.NotFound);
    }
}
