using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecurityService.DataTransferObjects.Requests;
using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers
{
    using System.Threading;
    using Exceptions;
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Models;
    using Requests;
    using Shared.Exceptions;

    public class RoleRequestHandler : IRequestHandler<SecurityServiceCommands.CreateRoleCommand, Result>,
                                      IRequestHandler<SecurityServiceQueries.GetRoleQuery, Result<RoleDetails>>,
                                      IRequestHandler<SecurityServiceQueries.GetRolesQuery, Result<List<RoleDetails>>>{
        private readonly RoleManager<IdentityRole> RoleManager;

        public RoleRequestHandler(RoleManager<IdentityRole> roleManager){
            this.RoleManager = roleManager;
        }
        public async Task<Result> Handle(SecurityServiceCommands.CreateRoleCommand command, CancellationToken cancellationToken){

            IdentityRole newIdentityRole = new IdentityRole
            {
                Id = command.RoleId.ToString(),
                Name = command.Name,
                NormalizedName = command.Name.ToUpper()
            };

            // Ensure role name is not a duplicate
            if (await this.RoleManager.RoleExistsAsync(newIdentityRole.Name))
            {
                return Result.Conflict($"Role {newIdentityRole.Name} already exists");
            }

            IdentityResult createResult = await this.RoleManager.CreateAsync(newIdentityRole);

            if (!createResult.Succeeded)
            {
                return Result.Failure($"Error creating role {newIdentityRole.Name} {createResult}");
            }
            return Result.Success();
        }

        public async Task<Result<RoleDetails>> Handle(SecurityServiceQueries.GetRoleQuery query, CancellationToken cancellationToken){
            IdentityRole identityRole = await this.RoleManager.FindByIdAsync(query.RoleId.ToString());

            if (identityRole == null)
            {
                return Result.NotFound($"No role found with Id {query.RoleId}");
            }

            // Role has been found
            RoleDetails response = new RoleDetails
                                   {
                                       RoleId = Guid.Parse(identityRole.Id),
                                       RoleName = identityRole.Name
                                   };

            return  Result.Success(response);
        }

        public async Task<Result<List<RoleDetails>>> Handle(SecurityServiceQueries.GetRolesQuery query, CancellationToken cancellationToken){
            List<RoleDetails> response = new List<RoleDetails>();

            IQueryable<IdentityRole> roleQuery = this.RoleManager.Roles;

            List<IdentityRole> roles = await roleQuery.ToListAsyncSafe(cancellationToken);

            foreach (IdentityRole identityRole in roles)
            {
                response.Add(new RoleDetails
                             {
                                 RoleId = Guid.Parse(identityRole.Id),
                                 RoleName = identityRole.Name
                             });
            }

            return Result.Success(response);
        }
    }
}
