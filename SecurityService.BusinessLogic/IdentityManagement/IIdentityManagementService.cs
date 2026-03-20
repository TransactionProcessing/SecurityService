namespace SecurityService.BusinessLogic.IdentityManagement;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Requests;
using SecurityService.Models;
using SimpleResults;

public interface IIdentityManagementService
{
    Task<Result> CreateApiResource(SecurityServiceCommands.CreateApiResourceCommand command, CancellationToken cancellationToken);
    Task<Result<ApiResource>> GetApiResource(SecurityServiceQueries.GetApiResourceQuery query, CancellationToken cancellationToken);
    Task<Result<List<ApiResource>>> GetApiResources(SecurityServiceQueries.GetApiResourcesQuery query, CancellationToken cancellationToken);

    Task<Result> CreateApiScope(SecurityServiceCommands.CreateApiScopeCommand command, CancellationToken cancellationToken);
    Task<Result<ApiScope>> GetApiScope(SecurityServiceQueries.GetApiScopeQuery query, CancellationToken cancellationToken);
    Task<Result<List<ApiScope>>> GetApiScopes(SecurityServiceQueries.GetApiScopesQuery query, CancellationToken cancellationToken);

    Task<Result> CreateClient(SecurityServiceCommands.CreateClientCommand command, CancellationToken cancellationToken);
    Task<Result<Client>> GetClient(SecurityServiceQueries.GetClientQuery query, CancellationToken cancellationToken);
    Task<Result<List<Client>>> GetClients(SecurityServiceQueries.GetClientsQuery query, CancellationToken cancellationToken);

    Task<Result> CreateIdentityResource(SecurityServiceCommands.CreateIdentityResourceCommand command, CancellationToken cancellationToken);
    Task<Result<IdentityResource>> GetIdentityResource(SecurityServiceQueries.GetIdentityResourceQuery query, CancellationToken cancellationToken);
    Task<Result<List<IdentityResource>>> GetIdentityResources(SecurityServiceQueries.GetIdentityResourcesQuery query, CancellationToken cancellationToken);

    Task<Result> CreateRole(SecurityServiceCommands.CreateRoleCommand command, CancellationToken cancellationToken);
    Task<Result<RoleDetails>> GetRole(SecurityServiceQueries.GetRoleQuery query, CancellationToken cancellationToken);
    Task<Result<List<RoleDetails>>> GetRoles(SecurityServiceQueries.GetRolesQuery query, CancellationToken cancellationToken);

    Task<Result> CreateUser(SecurityServiceCommands.CreateUserCommand command, CancellationToken cancellationToken);
    Task<Result<UserDetails>> GetUser(SecurityServiceQueries.GetUserQuery query, CancellationToken cancellationToken);
    Task<Result<List<UserDetails>>> GetUsers(SecurityServiceQueries.GetUsersQuery query, CancellationToken cancellationToken);
    Task<Result<ChangeUserPasswordResult>> ChangeUserPassword(SecurityServiceCommands.ChangeUserPasswordCommand command, CancellationToken cancellationToken);
    Task<Result> ConfirmUserEmailAddress(SecurityServiceCommands.ConfirmUserEmailAddressCommand command, CancellationToken cancellationToken);
    Task<Result<string>> ProcessPasswordResetConfirmation(SecurityServiceCommands.ProcessPasswordResetConfirmationCommand command, CancellationToken cancellationToken);
    Task<Result> ProcessPasswordResetRequest(SecurityServiceCommands.ProcessPasswordResetRequestCommand command, CancellationToken cancellationToken);
    Task<Result> SendWelcomeEmail(SecurityServiceCommands.SendWelcomeEmailCommand command, CancellationToken cancellationToken);
}
