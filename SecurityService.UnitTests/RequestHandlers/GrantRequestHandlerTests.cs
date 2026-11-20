using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SecurityService.BusinessLogic.Requests;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;
using SecurityService.Models;
using TransactionProcessing.UnionResults;

namespace SecurityService.UnitTests.RequestHandlers;

public class GrantRequestHandlerTests
{
    [Fact]
    public async Task GetUserGrants_WhenNoAuthorizations_ReturnsEmptyList()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.GetUserGrants_WhenNoAuthorizations_ReturnsEmptyList));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceQueries.GetUserGrantsQuery("user-1"));

        result.IsSuccess().ShouldBeTrue();
        result.Value.ShouldBeOfType<Success<List<GrantDetails>>>().Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<Success<List<GrantDetails>>>().Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task RevokeGrant_WhenAuthorizationNotFound_ReturnsNotFound()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.RevokeGrant_WhenAuthorizationNotFound_ReturnsNotFound));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.RevokeGrantCommand("user-1", "nonexistent-authorization-id"));

        result.IsSuccess().ShouldBeFalse();
        result.GetErrors().Single().ErrorType.ShouldBe(ErrorType.NotFound);
    }
}
