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
                                      IRequestHandler<GetRoleRequest, RoleDetails>,
                                      IRequestHandler<GetRolesRequest, List<RoleDetails>>{
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

        public async Task<RoleDetails> Handle(GetRoleRequest request, CancellationToken cancellationToken){
            IdentityRole identityRole = await this.RoleManager.FindByIdAsync(request.RoleId.ToString());

            if (identityRole == null)
            {
                throw new NotFoundException($"No role found with Id {request.RoleId}");
            }

            // Role has been found
            RoleDetails response = new RoleDetails
                                   {
                                       RoleId = Guid.Parse(identityRole.Id),
                                       RoleName = identityRole.Name
                                   };

            return response;
        }

        public async Task<List<RoleDetails>> Handle(GetRolesRequest request, CancellationToken cancellationToken){
            List<RoleDetails> response = new List<RoleDetails>();

            IQueryable<IdentityRole> query = this.RoleManager.Roles;

            List<IdentityRole> roles = await query.ToListAsyncSafe(cancellationToken);

            foreach (IdentityRole identityRole in roles)
            {
                response.Add(new RoleDetails
                             {
                                 RoleId = Guid.Parse(identityRole.Id),
                                 RoleName = identityRole.Name
                             });
            }

            return response;
        }
    }
}
