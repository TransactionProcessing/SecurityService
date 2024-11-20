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
    using Duende.IdentityServer.EntityFramework.DbContexts;
    using Duende.IdentityServer.EntityFramework.Entities;
    using Duende.IdentityServer.EntityFramework.Mappers;
    using Duende.IdentityServer.Models;
    using IdentityModel;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Requests;
    using Shared.Exceptions;
    using Client = Duende.IdentityServer.Models.Client;
    using Secret = Duende.IdentityServer.Models.Secret;

    public class ClientRequestHandler : IRequestHandler<SecurityServiceCommands.CreateClientCommand, Result>,
                                        IRequestHandler<GetClientRequest, Client>,
                                        IRequestHandler<GetClientsRequest, List<Client>>{
        private readonly ConfigurationDbContext ConfigurationDbContext;

        public ClientRequestHandler(ConfigurationDbContext configurationDbContext){
            this.ConfigurationDbContext = configurationDbContext;
        }

        public async Task<Result> Handle(SecurityServiceCommands.CreateClientCommand command, CancellationToken cancellationToken){
            // Validate the grant types list
            Result validationResult = this.ValidateGrantTypes(command.AllowedGrantTypes);
            if (validationResult.IsFailed) {
                return validationResult;
            }

            // Create the model from the request
            Client client = new Client
            {
                ClientId = command.ClientId,
                ClientName = command.ClientName,
                Description = command.ClientDescription,
                ClientSecrets ={
                                                             new Secret(command.Secret.ToSha256())
                                                         },
                AllowedGrantTypes = command.AllowedGrantTypes,
                AllowedScopes = command.AllowedScopes,
                RequireConsent = command.RequireConsent,
                AllowOfflineAccess = command.AllowOfflineAccess,
                ClientUri = command.ClientUri
            };

            if (command.AllowedGrantTypes.Contains("hybrid"))
            {
                client.RequirePkce = false;
            }

            if (command.ClientRedirectUris != null && command.ClientRedirectUris.Any())
            {
                client.RedirectUris = new List<String>();
                foreach (String clientRedirectUri in command.ClientRedirectUris)
                {
                    client.RedirectUris.Add(clientRedirectUri);
                }
            }

            if (command.ClientPostLogoutRedirectUris != null && command.ClientPostLogoutRedirectUris.Any())
            {
                client.PostLogoutRedirectUris = new List<String>();
                foreach (String clientPostLogoutRedirectUri in command.ClientPostLogoutRedirectUris)
                {
                    client.PostLogoutRedirectUris.Add(clientPostLogoutRedirectUri);
                }
            }

            // Now translate the model to the entity
            await this.ConfigurationDbContext.Clients.AddAsync(client.ToEntity(), cancellationToken);

            // Save the changes
            await this.ConfigurationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Client> Handle(GetClientRequest request, CancellationToken cancellationToken){
            Client clientModel = null;

            Duende.IdentityServer.EntityFramework.Entities.Client clientEntity = await this.ConfigurationDbContext.Clients.Include(c => c.AllowedGrantTypes)
                                                                                           .Include(c => c.AllowedScopes).Where(c => c.ClientId == request.ClientId)
                                                                                           .SingleOrDefaultAsync(cancellationToken: cancellationToken);

            if (clientEntity == null)
            {
                throw new NotFoundException($"No client found with Client Id [{request.ClientId}]");
            }

            clientModel = clientEntity.ToModel();

            return clientModel;
        }

        public async Task<List<Client>> Handle(GetClientsRequest request, CancellationToken cancellationToken){
            List<Client> clientModels = new List<Client>();

            List<Duende.IdentityServer.EntityFramework.Entities.Client> clientEntities = await this.ConfigurationDbContext.Clients.Include(c => c.AllowedGrantTypes)
                                                                                                   .Include(c => c.AllowedScopes)
                                                                                                   .ToListAsync(cancellationToken: cancellationToken);

            if (clientEntities.Any())
            {
                foreach (Duende.IdentityServer.EntityFramework.Entities.Client clientEntity in clientEntities)
                {
                    clientModels.Add(clientEntity.ToModel());
                }
            }

            return clientModels;
        }

        private Result ValidateGrantTypes(List<String> allowedGrantTypes){
            // Get a list of valid grant types
            List<String> validTypesList = new List<String>();

            validTypesList.Add(GrantType.AuthorizationCode);
            validTypesList.Add(GrantType.ClientCredentials);
            validTypesList.Add(GrantType.DeviceFlow);
            validTypesList.Add(GrantType.Hybrid);
            validTypesList.Add(GrantType.Implicit);
            validTypesList.Add(GrantType.ResourceOwnerPassword);

            List<String> invalidGrantTypes = allowedGrantTypes.Where(a => validTypesList.All(v => v != a)).ToList();

            if (invalidGrantTypes.Any()){
                return Result.Invalid($"The grant types [{String.Join(", ", invalidGrantTypes)}] are not valid to create a new client");
            }
            return Result.Success();
        }
    }
}
