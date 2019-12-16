namespace SecurityService.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.EntityFramework.Interfaces;
    using IdentityServer4.EntityFramework.Mappers;
    using IdentityServer4.Models;
    using Microsoft.EntityFrameworkCore.Internal;

    public interface ISecurityServiceManager
    {
        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="secret">The secret.</param>
        /// <param name="clientName">Name of the client.</param>
        /// <param name="clientDescription">The client description.</param>
        /// <param name="allowedScopes">The allowed scopes.</param>
        /// <param name="allowedGrantTypes">The allowed grant types.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<String> CreateClient(String clientId,
                                  String secret,
                                  String clientName,
                                  String clientDescription,
                                  List<String> allowedScopes,
                                  List<String> allowedGrantTypes,
                                  CancellationToken cancellationToken);

        Task<String> CreateApiResource(String name,
                                       String displayName,
                                       String description,
                                       String secret,
                                       List<String> scopes,
                                       List<String> userClaims,
                                       CancellationToken cancellationToken);
    }

    public class SecurityServiceManager : ISecurityServiceManager
    {
        private readonly ConfigurationDbContext ConfigurationDbContext;


        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityServiceManager" /> class.
        /// </summary>
        /// <param name="configurationDbContextResolver">The configuration database context resolver.</param>
        public SecurityServiceManager(ConfigurationDbContext configurationDbContext)
        {
            this.ConfigurationDbContext = configurationDbContext;
        }

        #endregion

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="secret">The secret.</param>
        /// <param name="clientName">Name of the client.</param>
        /// <param name="clientDescription">The client description.</param>
        /// <param name="allowedScopes">The allowed scopes.</param>
        /// <param name="allowedGrantTypes">The allowed grant types.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<String> CreateClient(String clientId,
                                               String secret,
                                               String clientName,
                                               String clientDescription,
                                               List<String> allowedScopes,
                                               List<String> allowedGrantTypes,
                                               CancellationToken cancellationToken)
        {
            // Validate the grant types list
            //this.ValidateGrantTypes(allowedGrantTypes);

            IConfigurationDbContext context = this.ConfigurationDbContext;

            // Create the model from the request
            Client client = new Client
            {
                ClientId = clientId,
                ClientName = clientName,
                Description = clientDescription,
                ClientSecrets =
                                {
                                    new Secret(secret.ToSha256())
                                },
                AllowedGrantTypes = allowedGrantTypes,
                AllowedScopes = allowedScopes
            };

            // Now translate the model to the entity
            await context.Clients.AddAsync(client.ToEntity(), cancellationToken);

            // Save the changes
            await context.SaveChangesAsync();

            return clientId;
        }

        public async Task<String> CreateApiResource(String name,
                                                    String displayName,
                                                    String description,
                                                    String secret,
                                                    List<String> scopes,
                                                    List<String> userClaims,
                                                    CancellationToken cancellationToken)
        {
            IConfigurationDbContext context = this.ConfigurationDbContext;
            ApiResource apiResource = new ApiResource
                                      {
                                          ApiSecrets = new List<Secret>
                                                       {
                                                           new Secret(secret.ToSha256())
                                                       },
                                          Description = description,
                                          DisplayName = displayName,
                                          Name = name,
                                          UserClaims = userClaims
                                      };

            if (scopes != null && scopes.Any())
            {
                foreach (String scope in scopes)
                {
                    apiResource.Scopes.Add(new Scope(scope));
                }
            }

            // Now translate the model to the entity
            await context.ApiResources.AddAsync(apiResource.ToEntity(), cancellationToken);

            // Save the changes
            await context.SaveChangesAsync();

            return name;
        }
    }
}