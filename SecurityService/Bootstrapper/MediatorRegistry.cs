namespace SecurityService.Bootstrapper;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BusinessLogic.RequestHandlers;
using BusinessLogic.Requests;
using Duende.IdentityServer.Models;
using Lamar;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

[ExcludeFromCodeCoverage]
public class MediatorRegistry : ServiceRegistry
{
    public MediatorRegistry()
    {
        this.AddTransient<IMediator, Mediator>();

        // request & notification handlers
        this.AddTransient<ServiceFactory>(context =>
                                          {
                                              return t => context.GetService(t);
                                          });

        this.AddSingleton<IRequestHandler<GetApiScopeRequest, ApiScope>, ApiScopeRequestHandler>();
        this.AddSingleton<IRequestHandler<GetApiScopesRequest, List<ApiScope>>, ApiScopeRequestHandler>();
        this.AddSingleton<IRequestHandler<CreateApiScopeRequest, Unit>, ApiScopeRequestHandler>();

        this.AddSingleton<IRequestHandler<GetApiResourceRequest, ApiResource>, ApiResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<GetApiResourcesRequest, List<ApiResource>>, ApiResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<CreateApiResourceRequest, Unit>, ApiResourceRequestHandler>();
    }
}