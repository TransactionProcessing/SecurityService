namespace SecurityService.BusinessLogic.RequestHandlers{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using Duende.IdentityServer.EntityFramework.Mappers;
    using Duende.IdentityServer.Models;
    using IdentityModel;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Requests;
    using Shared.Exceptions;

    public class ApiResourceRequestHandler : IRequestHandler<CreateApiResourceRequest>,
                                             IRequestHandler<GetApiResourceRequest, ApiResource>,
                                             IRequestHandler<GetApiResourcesRequest, List<ApiResource>>{
        #region Fields

        private readonly ConfigurationDbContext ConfigurationDbContext;

        #endregion

        #region Constructors

        public ApiResourceRequestHandler(ConfigurationDbContext configurationDbContext){
            this.ConfigurationDbContext = configurationDbContext;
        }

        #endregion

        #region Methods

        public async Task Handle(CreateApiResourceRequest request, CancellationToken cancellationToken){
            ApiResource apiResource = new ApiResource{
                                                         ApiSecrets = new List<Secret>{
                                                                                          new Secret(request.Secret.ToSha256())
                                                                                      },
                                                         Description = request.Description,
                                                         DisplayName = request.DisplayName,
                                                         Name = request.Name,
                                                         UserClaims = request.UserClaims,
                                                     };

            if (request.Scopes != null && request.Scopes.Any()){
                foreach (String scope in request.Scopes){
                    apiResource.Scopes.Add(scope);
                }
            }

            // Now translate the model to the entity
            await this.ConfigurationDbContext.ApiResources.AddAsync(apiResource.ToEntity(), cancellationToken);

            // Save the changes
            await this.ConfigurationDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<ApiResource> Handle(GetApiResourceRequest request, CancellationToken cancellationToken){
            ApiResource apiResourceModel = null;

            Duende.IdentityServer.EntityFramework.Entities.ApiResource apiResourceEntity = await this.ConfigurationDbContext.ApiResources
                                                                                                     .Where(a => a.Name == request.Name).Include(a => a.Scopes)
                                                                                                     .Include(a => a.UserClaims)
                                                                                                     .SingleOrDefaultAsync(cancellationToken:cancellationToken);

            if (apiResourceEntity == null){
                throw new NotFoundException($"No Api Resource found with Name [{request.Name}]");
            }

            apiResourceModel = apiResourceEntity.ToModel();

            return apiResourceModel;
        }

        public async Task<List<ApiResource>> Handle(GetApiResourcesRequest request, CancellationToken cancellationToken){
            List<ApiResource> apiResourceModels = new List<ApiResource>();

            List<Duende.IdentityServer.EntityFramework.Entities.ApiResource> apiResourceEntities = await this.ConfigurationDbContext.ApiResources.Include(a => a.Scopes)
                                                                                                             .Include(a => a.UserClaims)
                                                                                                             .ToListAsync(cancellationToken:cancellationToken);

            if (apiResourceEntities.Any()){
                foreach (Duende.IdentityServer.EntityFramework.Entities.ApiResource apiResourceEntity in apiResourceEntities){
                    apiResourceModels.Add(apiResourceEntity.ToModel());
                }
            }

            return apiResourceModels;
        }

        #endregion
    }
}