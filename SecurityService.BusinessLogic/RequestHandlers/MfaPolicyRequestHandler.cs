using MediatR;
using SecurityService.BusinessLogic.Mfa;
using SecurityService.BusinessLogic.Requests;
using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class MfaPolicyRequestHandler :
    IRequestHandler<SecurityServiceCommands.RequireUserMfaCommand, Result>,
    IRequestHandler<SecurityServiceCommands.RemoveUserMfaCommand, Result>,
    IRequestHandler<SecurityServiceCommands.RequireRoleMfaCommand, Result>,
    IRequestHandler<SecurityServiceCommands.RemoveRoleMfaCommand, Result>
{
    private readonly MfaPolicyService _mfaPolicyService;

    public MfaPolicyRequestHandler(MfaPolicyService mfaPolicyService)
    {
        _mfaPolicyService = mfaPolicyService;
    }

    public Task<Result> Handle(SecurityServiceCommands.RequireUserMfaCommand command, CancellationToken cancellationToken) =>
        _mfaPolicyService.RequireUserAsync(command.UserId, cancellationToken);

    public Task<Result> Handle(SecurityServiceCommands.RemoveUserMfaCommand command, CancellationToken cancellationToken) =>
        _mfaPolicyService.RemoveUserAsync(command.UserId, cancellationToken);

    public Task<Result> Handle(SecurityServiceCommands.RequireRoleMfaCommand command, CancellationToken cancellationToken) =>
        _mfaPolicyService.RequireRoleAsync(command.RoleId, cancellationToken);

    public Task<Result> Handle(SecurityServiceCommands.RemoveRoleMfaCommand command, CancellationToken cancellationToken) =>
        _mfaPolicyService.RemoveRoleAsync(command.RoleId, cancellationToken);
}
