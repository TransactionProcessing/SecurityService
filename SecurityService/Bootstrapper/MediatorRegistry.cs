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

        this.AddSingleton<IRequestHandler<SecurityServiceQueries.GetApiScopeQuery, Result<ApiScope>>, ApiScopeRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceQueries.GetApiScopesQuery, Result<List<ApiScope>>>, ApiScopeRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.CreateApiScopeCommand, Result>, ApiScopeRequestHandler>();

        this.AddSingleton<IRequestHandler<SecurityServiceQueries.GetApiResourceQuery, Result<ApiResource>>, ApiResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceQueries.GetApiResourcesQuery, Result<List<ApiResource>>>, ApiResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.CreateApiResourceCommand, Result>, ApiResourceRequestHandler>();

        this.AddSingleton<IRequestHandler<SecurityServiceQueries.GetRoleQuery, Result<RoleDetails>>, RoleRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceQueries.GetRolesQuery, Result<List<RoleDetails>>>, RoleRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.CreateRoleCommand, Result>, RoleRequestHandler>();

        this.AddSingleton<IRequestHandler<SecurityServiceQueries.GetClientQuery, Result<Client>>, ClientRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceQueries.GetClientsQuery, Result<List<Client>>>, ClientRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.CreateClientCommand, Result>, ClientRequestHandler>();

        this.AddSingleton<IRequestHandler<SecurityServiceQueries.GetIdentityResourceQuery, Result<IdentityResource>>, IdentityResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceQueries.GetIdentityResourcesQuery, Result<List<IdentityResource>>>, IdentityResourceRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.CreateIdentityResourceCommand, Result>, IdentityResourceRequestHandler>();

        this.AddSingleton<IRequestHandler<SecurityServiceQueries.GetUserQuery, Result<UserDetails>>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceQueries.GetUsersQuery, Result<List<UserDetails>>>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.CreateUserCommand, Result>, UserRequestHandler>();

        this.AddSingleton<IRequestHandler<SecurityServiceCommands.ChangeUserPasswordCommand, Result<ChangeUserPasswordResult>>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.ConfirmUserEmailAddressCommand, Result>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.ProcessPasswordResetConfirmationCommand, Result<String>>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.ProcessPasswordResetRequestCommand, Result>, UserRequestHandler>();
        this.AddSingleton<IRequestHandler<SecurityServiceCommands.SendWelcomeEmailCommand, Result>, UserRequestHandler>();
    }
}