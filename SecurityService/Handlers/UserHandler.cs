using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Models;
using TransactionProcessing.UnionResults.AspNetCore;
using TransactionProcessing.UnionResults;

namespace SecurityService.Handlers;

public static class UserHandler
{
    public static async Task<Result> RequireMfa(IMediator mediator,
                                                 [System.ComponentModel.Description("Stable identity ID of the user whose MFA requirement is changed.")] string userId,
                                                 CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SecurityServiceCommands.RequireUserMfaCommand(userId), cancellationToken);
        return result;
    }

    public static async Task<Result> RemoveMfa(IMediator mediator,
                                                [System.ComponentModel.Description("Stable identity ID of the user whose MFA requirement is removed.")] string userId,
                                                CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SecurityServiceCommands.RemoveUserMfaCommand(userId), cancellationToken);
        return result;
    }

    public static async Task<Result> CreateUser(IMediator mediator, CreateUserRequest request, CancellationToken cancellationToken) {
        SecurityServiceCommands.CreateUserCommand command = new(request.GivenName, request.MiddleName, request.FamilyName, request.UserName, request.Password, request.EmailAddress, request.PhoneNumber, request.Claims, request.Roles);
        
        Result result = await mediator.Send(command, cancellationToken);

        return result;
    }

    public static async Task<Result<UserResponse>> GetUser(IMediator mediator,
                                              [System.ComponentModel.Description("Stable identity ID of the user to retrieve.")] string userId,
                                              CancellationToken cancellationToken) {
        SecurityServiceQueries.GetUserQuery query = new(userId);

        Result<UserDetails> result = await mediator.Send(query, cancellationToken);

        return result.Map(ModelFactory.ConvertFrom);
    }

    public static async Task<Result<List<UserResponse>>> GetUsers(IMediator mediator, [System.ComponentModel.Description("Optional exact sign-in name to filter by; omit or leave empty to list all users.")] string? userName, CancellationToken cancellationToken) {
        SecurityServiceQueries.GetUsersQuery query = new(userName);

        Result<List<UserDetails>> result = await mediator.Send(query, cancellationToken);

        return result.Map(ModelFactory.ConvertFrom);
    }
}
