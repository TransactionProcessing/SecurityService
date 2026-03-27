using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Factories;
using Shared.Results.Web;

namespace SecurityService.Handlers;

public static class UserHandler
{
    public static async Task<IResult> CreateUser(IMediator mediator, CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SecurityServiceCommands.CreateUserCommand(
            request.GivenName,
            request.MiddleName,
            request.FamilyName,
            request.UserName,
            request.Password,
            request.EmailAddress,
            request.PhoneNumber,
            request.Claims,
            request.Roles), cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> GetUser(IMediator mediator, string userId, CancellationToken cancellationToken)
        => ResponseFactory.FromResult(await mediator.Send(new SecurityServiceQueries.GetUserQuery(userId), cancellationToken), ModelFactory.ConvertFrom);

    public static async Task<IResult> GetUsers(IMediator mediator, string? userName, CancellationToken cancellationToken)
        => ResponseFactory.FromResult(await mediator.Send(new SecurityServiceQueries.GetUsersQuery(userName), cancellationToken), ModelFactory.ConvertFrom);
}
