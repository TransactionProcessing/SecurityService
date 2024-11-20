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
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Requests;
    using Shared.Exceptions;

    public class ApiScopeRequestHandler : IRequestHandler<SecurityServiceCommands.CreateApiScopeCommand, Result>,
                                          IRequestHandler<GetApiScopeRequest, ApiScope>,
                                          IRequestHandler<GetApiScopesRequest, List<ApiScope>>{
        #region Fields

        private readonly ConfigurationDbContext ConfigurationDbContext;

        #endregion

        #region Constructors

        public ApiScopeRequestHandler(ConfigurationDbContext configurationDbContext){
            this.ConfigurationDbContext = configurationDbContext;
        }

        #endregion

        #region Methods

        public async Task<Result> Handle(SecurityServiceCommands.CreateApiScopeCommand command, CancellationToken cancellationToken){
            ApiScope apiScope = new ApiScope
            {
                Description = command.Description,
                DisplayName = command.DisplayName,
                Name = command.Name,
                Emphasize = false,
                Enabled = true,
                Required = false,
                ShowInDiscoveryDocument = true
            };

            // Now translate the model to the entity
            await this.ConfigurationDbContext.ApiScopes.AddAsync(apiScope.ToEntity(), cancellationToken);

            // Save the changes
            await this.ConfigurationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<ApiScope> Handle(GetApiScopeRequest request, CancellationToken cancellationToken){
            ApiScope apiScopeModel = null;

            Duende.IdentityServer.EntityFramework.Entities.ApiScope apiScopeEntity = await this.ConfigurationDbContext.ApiScopes.Where(a => a.Name == request.Name)
                                                                                               .Include(a => a.Properties).Include(a => a.UserClaims)
                                                                                               .SingleOrDefaultAsync(cancellationToken:cancellationToken);

            if (apiScopeEntity == null){
                throw new NotFoundException($"No Api Scope found with Name [{request.Name}]");
            }

            apiScopeModel = apiScopeEntity.ToModel();

            return apiScopeModel;
        }

        public async Task<List<ApiScope>> Handle(GetApiScopesRequest request, CancellationToken cancellationToken){
            List<ApiScope> apiScopeModels = new List<ApiScope>();

            List<Duende.IdentityServer.EntityFramework.Entities.ApiScope> apiScopeEntities = await this.ConfigurationDbContext.ApiScopes.Include(a => a.Properties)
                                                                                                       .Include(a => a.UserClaims)
                                                                                                       .ToListAsync(cancellationToken:cancellationToken);

            if (apiScopeEntities.Any()){
                foreach (Duende.IdentityServer.EntityFramework.Entities.ApiScope apiScopeEntity in apiScopeEntities){
                    apiScopeModels.Add(apiScopeEntity.ToModel());
                }
            }

            return apiScopeModels;
        }

        #endregion
    }
}