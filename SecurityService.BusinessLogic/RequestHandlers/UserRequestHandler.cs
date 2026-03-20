using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Models;
using Requests;
using SecurityService.BusinessLogic.IdentityManagement;

public class UserRequestHandler : IRequestHandler<SecurityServiceCommands.CreateUserCommand, Result>,
                                  IRequestHandler<SecurityServiceQueries.GetUserQuery, Result<UserDetails>>,
                                  IRequestHandler<SecurityServiceQueries.GetUsersQuery, Result<List<UserDetails>>>,
                                  IRequestHandler<SecurityServiceCommands.ChangeUserPasswordCommand, Result<ChangeUserPasswordResult>>,
                                  IRequestHandler<SecurityServiceCommands.ConfirmUserEmailAddressCommand, Result>,
                                  IRequestHandler<SecurityServiceCommands.ProcessPasswordResetConfirmationCommand, Result<string>>,
                                  IRequestHandler<SecurityServiceCommands.ProcessPasswordResetRequestCommand, Result>,
                                  IRequestHandler<SecurityServiceCommands.SendWelcomeEmailCommand, Result>
{
    private readonly IIdentityManagementService IdentityManagementService;

    public UserRequestHandler(IIdentityManagementService identityManagementService)
    {
        this.IdentityManagementService = identityManagementService;
    }

    public Task<Result> Handle(SecurityServiceCommands.CreateUserCommand command, CancellationToken cancellationToken) =>
        this.IdentityManagementService.CreateUser(command, cancellationToken);

    public Task<Result<UserDetails>> Handle(SecurityServiceQueries.GetUserQuery query, CancellationToken cancellationToken) =>
        this.IdentityManagementService.GetUser(query, cancellationToken);

    public Task<Result<List<UserDetails>>> Handle(SecurityServiceQueries.GetUsersQuery query, CancellationToken cancellationToken) =>
        this.IdentityManagementService.GetUsers(query, cancellationToken);

    public Task<Result<ChangeUserPasswordResult>> Handle(SecurityServiceCommands.ChangeUserPasswordCommand command, CancellationToken cancellationToken) =>
        this.IdentityManagementService.ChangeUserPassword(command, cancellationToken);

    public Task<Result> Handle(SecurityServiceCommands.ConfirmUserEmailAddressCommand command, CancellationToken cancellationToken) =>
        this.IdentityManagementService.ConfirmUserEmailAddress(command, cancellationToken);

    public Task<Result<string>> Handle(SecurityServiceCommands.ProcessPasswordResetConfirmationCommand command, CancellationToken cancellationToken) =>
        this.IdentityManagementService.ProcessPasswordResetConfirmation(command, cancellationToken);

    public Task<Result> Handle(SecurityServiceCommands.ProcessPasswordResetRequestCommand command, CancellationToken cancellationToken) =>
        this.IdentityManagementService.ProcessPasswordResetRequest(command, cancellationToken);

    public Task<Result> Handle(SecurityServiceCommands.SendWelcomeEmailCommand command, CancellationToken cancellationToken) =>
        this.IdentityManagementService.SendWelcomeEmail(command, cancellationToken);
}
