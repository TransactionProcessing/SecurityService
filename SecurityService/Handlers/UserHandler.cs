using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SecurityService.BusinessLogic.Requests;
using Shared.Results;
using Shared.Results.Web;
using SimpleResults;

namespace SecurityService.Handlers
{
    using DataTransferObjects;
    using Factories;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using SecurityService.BusinessLogic;

    public static class UserHandler
    {
        public static async Task<IResult> CreateUser(IMediator mediator,
                                                     CreateUserRequest createUserRequest,
                                                     CancellationToken cancellationToken)
        {
            Guid userId = Guid.NewGuid();

            SecurityServiceCommands.CreateUserCommand command = new(
                userId,
                createUserRequest.GivenName,
                createUserRequest.MiddleName,
                createUserRequest.FamilyName,
                createUserRequest.EmailAddress,
                createUserRequest.Password,
                createUserRequest.EmailAddress,
                createUserRequest.PhoneNumber,
                createUserRequest.Claims,
                createUserRequest.Roles);

            Result result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        public static async Task<IResult> GetUser(IMediator mediator,
                                                  Guid userId,
                                                  CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetUserQuery query = new(userId);

            Result<Models.UserDetails> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }

        public static async Task<IResult> GetUsers(IMediator mediator,
                                                    string? userName,
                                                    CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetUsersQuery query = new(userName);

            Result<List<Models.UserDetails>> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }
    }
}