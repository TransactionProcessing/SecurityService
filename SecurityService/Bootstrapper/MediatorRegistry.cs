namespace SecurityService.Bootstrapper;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BusinessLogic.RequestHandlers;
using BusinessLogic.Requests;
using Duende.IdentityServer.Models;
using Lamar;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Models;

[ExcludeFromCodeCoverage]
public class MediatorRegistry : ServiceRegistry
{
    public MediatorRegistry()
    {
        this.AddTransient<IMediator, Mediator>();

        // request & notification handlers
        this.AddTransient<ServiceFactory>(context => context.GetService);

        this.AddSingleton<IRequestHandler<GetApiScopeRequest, ApiScope>, ApiScopeRequestHandler>();
        this.AddSingleton<IRequestHandler<GetApiScopesRequest, List<ApiScope>>, ApiScopeRequestHandler>();
        this.AddSingleton<IRequestHandler<CreateApiScopeRequest, Unit>, ApiScopeRequestHandler>();

        this.AddSingleton<IRequestHandler<GetApiResourceRequest, ApiResource>, ApiResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<GetApiResourcesRequest, List<ApiResource>>, ApiResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<CreateApiResourceRequest, Unit>, ApiResourceRequestHandler>();

        this.AddSingleton<IRequestHandler<GetRoleRequest, RoleDetails>, RoleRequestHandler>();
        this.AddSingleton<IRequestHandler<GetRolesRequest, List<RoleDetails>>, RoleRequestHandler>();
        this.AddSingleton<IRequestHandler<CreateRoleRequest>, RoleRequestHandler>();

        this.AddSingleton<IRequestHandler<GetClientRequest, Client>, ClientRequestHandler>();
        this.AddSingleton<IRequestHandler<GetClientsRequest, List<Client>>, ClientRequestHandler>();
        this.AddSingleton<IRequestHandler<CreateClientRequest, Unit>, ClientRequestHandler>();

        this.AddSingleton<IRequestHandler<GetIdentityResourceRequest, IdentityResource>, IdentityResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<GetIdentityResourcesRequest, List<IdentityResource>>, IdentityResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<CreateIdentityResourceRequest>, IdentityResourceRequestHandler>();

        this.AddSingleton<IRequestHandler<GetUserRequest, UserDetails>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<GetUsersRequest, List<UserDetails>>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<CreateUserRequest>, UserRequestHandler>();

        this.AddSingleton<IRequestHandler<ChangeUserPasswordRequest, (Boolean, String)>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<ConfirmUserEmailAddressRequest, Boolean>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<ProcessPasswordResetConfirmationRequest, String>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<ProcessPasswordResetRequest>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<SendWelcomeEmailRequest>, UserRequestHandler>();
    }
}