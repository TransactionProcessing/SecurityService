using SecurityService.DataTransferObjects.Requests;
using SimpleResults;

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

    public class ApiResourceRequestHandler : IRequestHandler<SecurityServiceCommands.CreateApiResourceCommand, Result>,
                                             IRequestHandler<SecurityServiceQueries.GetApiResourceQuery, Result<ApiResource>>,
                                             IRequestHandler<SecurityServiceQueries.GetApiResourcesQuery, Result<List<ApiResource>>>{
        #region Fields

        private readonly ConfigurationDbContext ConfigurationDbContext;

        #endregion

        #region Constructors

        public ApiResourceRequestHandler(ConfigurationDbContext configurationDbContext){
            this.ConfigurationDbContext = configurationDbContext;
        }

        #endregion

        #region Methods

        public async Task<Result> Handle(SecurityServiceCommands.CreateApiResourceCommand command, CancellationToken cancellationToken) {
            ApiResource apiResource = new ApiResource
            {
                ApiSecrets = new List<Secret>{
                                                                                          new Secret(command.Secret.ToSha256())
                                                                                      },
                Description = command.Description,
                DisplayName = command.DisplayName,
                Name = command.Name,
                UserClaims = command.UserClaims,
            };

            if (command.Scopes != null && command.Scopes.Any())
            {
                foreach (String scope in command.Scopes)
                {
                    apiResource.Scopes.Add(scope);
                }
            }

            // Now translate the model to the entity
            await this.ConfigurationDbContext.ApiResources.AddAsync(apiResource.ToEntity(), cancellationToken);

            // Save the changes
            await this.ConfigurationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result<ApiResource>> Handle(SecurityServiceQueries.GetApiResourceQuery query, CancellationToken cancellationToken){
            ApiResource apiResourceModel = null;

            Duende.IdentityServer.EntityFramework.Entities.ApiResource apiResourceEntity = await this.ConfigurationDbContext.ApiResources
                                                                                                     .Where(a => a.Name == query.Name).Include(a => a.Scopes)
                                                                                                     .Include(a => a.UserClaims)
                                                                                                     .SingleOrDefaultAsync(cancellationToken:cancellationToken);

            if (apiResourceEntity == null){
                return Result.NotFound($"No Api Resource found with Name [{query.Name}]");
            }

            apiResourceModel = apiResourceEntity.ToModel();

            return Result.Success(apiResourceModel);
        }

        public async Task<Result<List<ApiResource>>> Handle(SecurityServiceQueries.GetApiResourcesQuery request, CancellationToken cancellationToken){
            List<ApiResource> apiResourceModels = new List<ApiResource>();

            List<Duende.IdentityServer.EntityFramework.Entities.ApiResource> apiResourceEntities = await this.ConfigurationDbContext.ApiResources.Include(a => a.Scopes)
                                                                                                             .Include(a => a.UserClaims)
                                                                                                             .ToListAsync(cancellationToken:cancellationToken);

            if (apiResourceEntities.Any()){
                foreach (Duende.IdentityServer.EntityFramework.Entities.ApiResource apiResourceEntity in apiResourceEntities){
                    apiResourceModels.Add(apiResourceEntity.ToModel());
                }
            }

            return Result.Success(apiResourceModels);
        }

        #endregion
    }
}