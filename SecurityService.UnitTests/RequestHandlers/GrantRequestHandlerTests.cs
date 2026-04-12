using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SecurityService.BusinessLogic.Requests;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;
using SimpleResults;

namespace SecurityService.UnitTests.RequestHandlers;

public class GrantRequestHandlerTests
{
    [Fact]
    public async Task GetUserGrants_WhenNoAuthorizations_ReturnsEmptyList()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.GetUserGrants_WhenNoAuthorizations_ReturnsEmptyList));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceQueries.GetUserGrantsQuery("user-1"));

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldBeEmpty();
    }

    [Fact]
    public async Task RevokeGrant_WhenAuthorizationNotFound_ReturnsNotFound()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.RevokeGrant_WhenAuthorizationNotFound_ReturnsNotFound));
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SecurityServiceCommands.RevokeGrantCommand("user-1", "nonexistent-authorization-id"));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
}
