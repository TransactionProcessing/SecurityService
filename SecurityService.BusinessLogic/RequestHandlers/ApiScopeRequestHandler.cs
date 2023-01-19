using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;

namespace SecurityService.BusinessLogic.RequestHandlers
{
    public class ApiScopeRequestHandler : IRequestHandler<CreateApiScopeRequest>,
        IRequestHandler<GetApiScopeRequest, ApiScope>,
        IRequestHandler<GetApiScopesRequest, List<ApiScope>>
    {
        private readonly ConfigurationDbContext ConfigurationDbContext;

        public ApiScopeRequestHandler(ConfigurationDbContext configurationDbContext)
        {
            ConfigurationDbContext = configurationDbContext;
        }
        public async Task<Unit> Handle(CreateApiScopeRequest request, CancellationToken cancellationToken)
        {
            ApiScope apiScope = new ApiScope
            {
                Description = request.Description,
                DisplayName = request.DisplayName,
                Name = request.Name,
                Emphasize = false,
                Enabled = true,
                Required = false,
                ShowInDiscoveryDocument = true
            };

            // Now translate the model to the entity
            await this.ConfigurationDbContext.ApiScopes.AddAsync(apiScope.ToEntity(), cancellationToken);

            // Save the changes
            await this.ConfigurationDbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        public async Task<ApiScope> Handle(GetApiScopeRequest request, CancellationToken cancellationToken)
        {
            ApiScope apiScopeModel = null;

            Duende.IdentityServer.EntityFramework.Entities.ApiScope apiScopeEntity = await this.ConfigurationDbContext.ApiScopes.Where(a => a.Name == request.Name)
                .Include(a => a.Properties).Include(a => a.UserClaims)
                .SingleOrDefaultAsync(cancellationToken: cancellationToken);

            if (apiScopeEntity == null)
            {
                throw new NotFoundException($"No Api Scope found with Name [{request.Name}]");
            }

            apiScopeModel = apiScopeEntity.ToModel();

            return apiScopeModel;
        }

        public async Task<List<ApiScope>> Handle(GetApiScopesRequest request, CancellationToken cancellationToken)
        {
            List<ApiScope> apiScopeModels = new List<ApiScope>();

            List<Duende.IdentityServer.EntityFramework.Entities.ApiScope> apiScopeEntities = await this.ConfigurationDbContext.ApiScopes.Include(a => a.Properties)
                .Include(a => a.UserClaims)
                .ToListAsync(cancellationToken: cancellationToken);

            if (apiScopeEntities.Any())
            {
                foreach (Duende.IdentityServer.EntityFramework.Entities.ApiScope apiScopeEntity in apiScopeEntities)
                {
                    apiScopeModels.Add(apiScopeEntity.ToModel());
                }
            }

            return apiScopeModels;
        }
    }


    public class GetApiScopesRequest : IRequest<List<ApiScope>>
    {
        public GetApiScopesRequest()
        {
        }

        public static GetApiScopesRequest Create()
        {
            return new GetApiScopesRequest();
        }
    }

    public class GetApiScopeRequest : IRequest<ApiScope>
    {
        public String Name { get; }

        public GetApiScopeRequest(String name)
        {
            Name = name;
        }

        public static GetApiScopeRequest Create(String name)
        {
            return new GetApiScopeRequest(name);
        }
    }

    public class CreateApiScopeRequest : IRequest<Unit>
    {
        public CreateApiScopeRequest(String name, String displayName,String description)
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
        }

        public String Name { get; }
        public String DisplayName { get; }
        public String Description { get; }

        public static CreateApiScopeRequest Create(String name, String displayName, String description)
        {
            return new CreateApiScopeRequest(name, displayName, description);
        }
        
    }
}
