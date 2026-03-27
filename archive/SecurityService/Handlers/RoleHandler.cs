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
    using DataTransferObjects.Requests;
    using Factories;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using SecurityService.BusinessLogic;

    public static class RoleHandler
    {
        public static async Task<IResult> CreateRole(IMediator mediator,
                                                     CreateRoleRequest createRoleRequest,
                                                     CancellationToken cancellationToken)
        {
            Guid roleId = Guid.NewGuid();

            SecurityServiceCommands.CreateRoleCommand command = new(roleId, createRoleRequest.RoleName);

            Result result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        public static async Task<IResult> GetRole(IMediator mediator,
                                                  Guid roleId,
                                                  CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetRoleQuery query = new(roleId);

            Result<Models.RoleDetails> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }

        public static async Task<IResult> GetRoles(IMediator mediator,
                                                    CancellationToken cancellationToken)
        {
            SecurityServiceQueries.GetRolesQuery query = new();

            Result<List<Models.RoleDetails>> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }
    }
}