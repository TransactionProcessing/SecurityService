using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecurityService.DataTransferObjects.Requests;
using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers
{
    using System.Security.Claims;
    using System.Threading;
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using Duende.IdentityServer.EntityFramework.Mappers;
    using Duende.IdentityServer.Models;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Requests;
    using Shared.Exceptions;

    public class IdentityResourceRequestHandler : IRequestHandler<SecurityServiceCommands.CreateIdentityResourceCommand, Result>,
                                                  IRequestHandler<SecurityServiceQueries.GetIdentityResourceQuery, Result<IdentityResource>>,
                                                  IRequestHandler<SecurityServiceQueries.GetIdentityResourcesQuery, Result<List<IdentityResource>>>{
        private readonly ConfigurationDbContext ConfigurationDbContext;

        public IdentityResourceRequestHandler(ConfigurationDbContext configurationDbContext){
            this.ConfigurationDbContext = configurationDbContext;
        }

        public async Task<Result> Handle(SecurityServiceCommands.CreateIdentityResourceCommand command, CancellationToken cancellationToken){
            IdentityResource identityResource = new IdentityResource(command.Name, command.DisplayName, command.Claims);
            identityResource.Emphasize = command.Emphasize;
            identityResource.Required = command.Required;
            identityResource.ShowInDiscoveryDocument = command.ShowInDiscoveryDocument;
            identityResource.Description = command.Description;

            // Now translate the model to the entity
            await this.ConfigurationDbContext.IdentityResources.AddAsync(identityResource.ToEntity(), cancellationToken);

            // Save the changes
            await this.ConfigurationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result<IdentityResource>> Handle(SecurityServiceQueries.GetIdentityResourceQuery query, CancellationToken cancellationToken){
            IdentityResource identityResourceModel = null;

            Duende.IdentityServer.EntityFramework.Entities.IdentityResource identityResourceEntity = await this.ConfigurationDbContext.IdentityResources
                                                                                                               .Where(a => a.Name == query.IdentityResourceName)
                                                                                                               .Include(a => a.UserClaims)
                                                                                                               .SingleOrDefaultAsync(cancellationToken: cancellationToken);

            if (identityResourceEntity == null)
            {
                return Result.NotFound($"No Identity Resource found with Name [{query.IdentityResourceName}]");
            }

            identityResourceModel = identityResourceEntity.ToModel();

            return Result.Success(identityResourceModel);
        }

        public async Task<Result<List<IdentityResource>>> Handle(SecurityServiceQueries.GetIdentityResourcesQuery query, CancellationToken cancellationToken){
            List<IdentityResource> identityResourceModels = new List<IdentityResource>();

            List<Duende.IdentityServer.EntityFramework.Entities.IdentityResource> identityResourceEntities =
                await this.ConfigurationDbContext.IdentityResources.Include(a => a.UserClaims).ToListAsync(cancellationToken: cancellationToken);

            if (identityResourceEntities.Any())
            {
                foreach (Duende.IdentityServer.EntityFramework.Entities.IdentityResource identityResourceEntity in identityResourceEntities)
                {
                    identityResourceModels.Add(identityResourceEntity.ToModel());
                }
            }

            return Result.Success(identityResourceModels);
        }
    }
}
