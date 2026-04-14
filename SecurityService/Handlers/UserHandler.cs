using MediatR;
using SecurityService.BusinessLogic.Requests;
using SecurityService.DataTransferObjects;
using SecurityService.Factories;
using SecurityService.Models;
using Shared.Results.Web;
using SimpleResults;

namespace SecurityService.Handlers;

public static class UserHandler
{
    public static async Task<IResult> CreateUser(IMediator mediator, CreateUserRequest request, CancellationToken cancellationToken) {
        SecurityServiceCommands.CreateUserCommand command = new(request.GivenName, request.MiddleName, request.FamilyName, request.UserName, request.Password, request.EmailAddress, request.PhoneNumber, request.Claims, request.Roles);
        
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> GetUser(IMediator mediator,
                                              string userId,
                                              CancellationToken cancellationToken) {
        SecurityServiceQueries.GetUserQuery query = new(userId);

        Result<UserDetails> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetUsers(IMediator mediator, string? userName, CancellationToken cancellationToken) {
        SecurityServiceQueries.GetUsersQuery query = new(userName);

        Result<List<UserDetails>> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }
}