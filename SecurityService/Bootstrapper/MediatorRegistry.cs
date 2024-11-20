using SimpleResults;

namespace SecurityService.Bootstrapper;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
        //this.AddTransient<ServiceFactory>(context => context.GetService);
        //this.AddMediatR(configuration => {
        //                    configuration.RegisterServicesFromAssembly(typeof(BusinessLogic.UserOptions).Assembly);
        //                    //configuration.AutoRegisterRequestProcessors = true;
        //                });

        this.AddSingleton<IRequestHandler<GetApiScopeRequest, ApiScope>, ApiScopeRequestHandler>();
        this.AddSingleton<IRequestHandler<GetApiScopesRequest, List<ApiScope>>, ApiScopeRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.CreateApiScopeCommand, Result>, ApiScopeRequestHandler>();

        this.AddSingleton<IRequestHandler<GetApiResourceRequest, ApiResource>, ApiResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<GetApiResourcesRequest, List<ApiResource>>, ApiResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.CreateApiResourceCommand, Result>, ApiResourceRequestHandler>();

        this.AddSingleton<IRequestHandler<GetRoleRequest, RoleDetails>, RoleRequestHandler>();
        this.AddSingleton<IRequestHandler<GetRolesRequest, List<RoleDetails>>, RoleRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.CreateRoleCommand, Result>, RoleRequestHandler>();

        this.AddSingleton<IRequestHandler<GetClientRequest, Client>, ClientRequestHandler>();
        this.AddSingleton<IRequestHandler<GetClientsRequest, List<Client>>, ClientRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.CreateClientCommand, Result>, ClientRequestHandler>();

        this.AddSingleton<IRequestHandler<GetIdentityResourceRequest, IdentityResource>, IdentityResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<GetIdentityResourcesRequest, List<IdentityResource>>, IdentityResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.CreateIdentityResourceCommand, Result>, IdentityResourceRequestHandler>();

        this.AddSingleton<IRequestHandler<GetUserRequest, UserDetails>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<GetUsersRequest, List<UserDetails>>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.CreateUserCommand, Result>, UserRequestHandler>();

        this.AddSingleton<IRequestHandler<SecurityServiceCommands.ChangeUserPasswordCommand, Result<ChangeUserPasswordResult>>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.ConfirmUserEmailAddressCommand, Result>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.ProcessPasswordResetConfirmationCommand, Result<String>>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.ProcessPasswordResetRequestCommand, Result>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.SendWelcomeEmailCommand, Result>, UserRequestHandler>();
    }
}