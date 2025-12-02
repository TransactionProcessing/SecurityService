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

            var command = new SecurityServiceCommands.CreateUserCommand(
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
                                                  IModelFactory modelFactory,
                                                  Guid userId,
                                                  CancellationToken cancellationToken)
        {
            var query = new SecurityServiceQueries.GetUserQuery(userId);

            Result<Models.UserDetails> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, modelFactory.ConvertFrom);
        }

        public static async Task<IResult> GetUsers(IMediator mediator,
                                                    IModelFactory modelFactory,
                                                    string? userName,
                                                    CancellationToken cancellationToken)
        {
            var query = new SecurityServiceQueries.GetUsersQuery(userName);

            Result<List<Models.UserDetails>> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, modelFactory.ConvertFrom);
        }
    }
}