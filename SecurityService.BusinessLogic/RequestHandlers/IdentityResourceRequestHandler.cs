using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class IdentityResourceRequestHandler : IRequestHandler<CreateIdentityResourceRequest>,
                                                  IRequestHandler<GetIdentityResourceRequest, IdentityResource>,
                                                  IRequestHandler<GetIdentityResourcesRequest, List<IdentityResource>>{
        private readonly ConfigurationDbContext ConfigurationDbContext;

        public IdentityResourceRequestHandler(ConfigurationDbContext configurationDbContext){
            this.ConfigurationDbContext = configurationDbContext;
        }

        public async Task<Unit> Handle(CreateIdentityResourceRequest request, CancellationToken cancellationToken){
            IdentityResource identityResource = new IdentityResource(request.Name, request.DisplayName, request.Claims);
            identityResource.Emphasize = request.Emphasize;
            identityResource.Required = request.Required;
            identityResource.ShowInDiscoveryDocument = request.ShowInDiscoveryDocument;
            identityResource.Description = request.Description;

            // Now translate the model to the entity
            await this.ConfigurationDbContext.IdentityResources.AddAsync(identityResource.ToEntity(), cancellationToken);

            // Save the changes
            await this.ConfigurationDbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        public async Task<IdentityResource> Handle(GetIdentityResourceRequest request, CancellationToken cancellationToken){
            IdentityResource identityResourceModel = null;

            Duende.IdentityServer.EntityFramework.Entities.IdentityResource identityResourceEntity = await this.ConfigurationDbContext.IdentityResources
                                                                                                               .Where(a => a.Name == request.IdentityResourceName)
                                                                                                               .Include(a => a.UserClaims)
                                                                                                               .SingleOrDefaultAsync(cancellationToken: cancellationToken);

            if (identityResourceEntity == null)
            {
                throw new NotFoundException($"No Identity Resource found with Name [{request.IdentityResourceName}]");
            }

            identityResourceModel = identityResourceEntity.ToModel();

            return identityResourceModel;
        }

        public async Task<List<IdentityResource>> Handle(GetIdentityResourcesRequest request, CancellationToken cancellationToken){
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

            return identityResourceModels;
        }
    }
}
