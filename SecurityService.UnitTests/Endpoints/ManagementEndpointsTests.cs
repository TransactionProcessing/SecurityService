using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Authorization;
using SecurityService.Endpoints;
using Shouldly;

namespace SecurityService.UnitTests.Endpoints;

public sealed class ManagementEndpointsTests
{
    [Fact]
    public async Task MapManagementEndpoints_AppliesManagementPolicyToEveryRoute()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(SecurityServiceCommands).Assembly));
        using var app = builder.Build();

        app.MapManagementEndpoints();
        await app.StartAsync();

        var endpoints = app.Services.GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .ToArray();

        endpoints.ShouldNotBeEmpty();
        endpoints.All(endpoint => endpoint.Metadata.GetMetadata<IAuthorizeData>() is { Policy: ManagementAuthorizationPolicies.ManagementApi })
            .ShouldBeTrue();
    }
}
