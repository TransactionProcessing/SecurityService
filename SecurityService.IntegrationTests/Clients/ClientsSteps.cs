namespace SecurityService.IntegrationTests.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using DataTransferObjects.Requests;
    using DataTransferObjects.Responses;
    using IntergrationTests.Common;
    using Microsoft.EntityFrameworkCore.Design;
    using Newtonsoft.Json;
    using Shouldly;
    using TechTalk.SpecFlow;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
    [Scope(Tag = "clients")]
    public class ClientsSteps
    {
        #region Fields

        /// <summary>
        /// The testing context
        /// </summary>
        private readonly TestingContext TestingContext;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsSteps"/> class.
        /// </summary>
        /// <param name="testingContext">The testing context.</param>
        public ClientsSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Givens the i create the following clients.
        /// </summary>
        /// <param name="table">The table.</param>
        [Given(@"I create the following clients")]
        public async Task GivenICreateTheFollowingClients(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                // Get the scopes
                String scopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
                // Get the grant types
                String grantTypes = SpecflowTableHelper.GetStringRowValue(tableRow, "GrantTypes");
                // Get the redirect uris
                String redirectUris = SpecflowTableHelper.GetStringRowValue(tableRow, "RedirectUris");
                // Get the post logout redirect uris
                String postLogoutRedirectUris = SpecflowTableHelper.GetStringRowValue(tableRow, "PostLogoutRedirectUris");
                
                CreateClientRequest createClientRequest = new CreateClientRequest
                                                          {
                                                              ClientId = SpecflowTableHelper.GetStringRowValue(tableRow, "ClientId"),
                                                              Secret = SpecflowTableHelper.GetStringRowValue(tableRow, "Secret"),
                                                              ClientName = SpecflowTableHelper.GetStringRowValue(tableRow, "Name"),
                                                              AllowedScopes = string.IsNullOrEmpty(scopes) ? null : scopes.Split(",").ToList(),
                                                              AllowedGrantTypes = string.IsNullOrEmpty(grantTypes) ? null : grantTypes.Split(",").ToList(),
                                                              ClientRedirectUris = string.IsNullOrEmpty(redirectUris) ? null : redirectUris.Split(",").ToList(),
                                                              ClientPostLogoutRedirectUris = string.IsNullOrEmpty(postLogoutRedirectUris) ? null : postLogoutRedirectUris.Split(",").ToList(),
                                                              ClientDescription = SpecflowTableHelper.GetStringRowValue(tableRow, "Description"),
                                                              RequireConsent = SpecflowTableHelper.GetBooleanValue(tableRow, "RequireConsent")
                                                          };
                
                CreateClientResponse createClientResponse = await this.CreateClient(createClientRequest, CancellationToken.None).ConfigureAwait(false);

                createClientResponse.ShouldNotBeNull();
                createClientResponse.ClientId.ShouldNotBeNullOrEmpty();

                this.TestingContext.Clients.Add(createClientResponse.ClientId);
            }
        }

        /// <summary>
        /// Whens the i get the clients clients details are returned as follows.
        /// </summary>
        /// <param name="numberOfClients">The number of clients.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the clients (.*) clients details are returned as follows")]
        public async Task WhenIGetTheClientsClientsDetailsAreReturnedAsFollows(Int32 numberOfClients,
                                                                               Table table)
        {
            List<ClientDetails> clientDetailsList = await this.GetClients(CancellationToken.None).ConfigureAwait(false);
            clientDetailsList.Count.ShouldBe(numberOfClients);
            foreach (TableRow tableRow in table.Rows)
            {
                String clientId = SpecflowTableHelper.GetStringRowValue(tableRow, "ClientId");
                ClientDetails clientDetails = clientDetailsList.SingleOrDefault(u => u.ClientId == clientId);

                String scopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
                String grantTypes = SpecflowTableHelper.GetStringRowValue(tableRow, "GrantTypes");

                clientDetails.ClientDescription.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Description"));
                clientDetails.ClientId.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "ClientId"));
                clientDetails.ClientName.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Name"));
                if (string.IsNullOrEmpty(scopes))
                {
                    clientDetails.AllowedScopes.ShouldBeEmpty();
                }
                else
                {
                    clientDetails.AllowedScopes.ShouldBe(scopes.Split(",").ToList());
                }

                if (string.IsNullOrEmpty(grantTypes))
                {
                    clientDetails.AllowedGrantTypes.ShouldBeEmpty();
                }
                else
                {
                    clientDetails.AllowedGrantTypes.ShouldBe(grantTypes.Split(",").ToList());
                }
            }
        }

        /// <summary>
        /// Whens the i get the client with client identifier the client details are returned as follows.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the client with client id '(.*)' the client details are returned as follows")]
        public async Task WhenIGetTheClientWithClientIdTheClientDetailsAreReturnedAsFollows(String clientId,
                                                                                            Table table)
        {
            ClientDetails clientDetails = await this.GetClient(clientId, CancellationToken.None).ConfigureAwait(false);

            table.Rows.Count.ShouldBe(1);
            TableRow tableRow = table.Rows.First();
            clientDetails.ShouldNotBeNull();

            String scopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
            String grantTypes = SpecflowTableHelper.GetStringRowValue(tableRow, "GrantTypes");

            clientDetails.ClientDescription.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Description"));
            clientDetails.ClientId.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "ClientId"));
            clientDetails.ClientName.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Name"));
            if (string.IsNullOrEmpty(scopes))
            {
                clientDetails.AllowedScopes.ShouldBeEmpty();
            }
            else
            {
                clientDetails.AllowedScopes.ShouldBe(scopes.Split(",").ToList());
            }

            if (string.IsNullOrEmpty(grantTypes))
            {
                clientDetails.AllowedGrantTypes.ShouldBeEmpty();
            }
            else
            {
                clientDetails.AllowedGrantTypes.ShouldBe(grantTypes.Split(",").ToList());
            }
        }

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="createClientRequest">The create client request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Http Status Code [{response.StatusCode}] Message [{responseBody}]</exception>
        private async Task<CreateClientResponse> CreateClient(CreateClientRequest createClientRequest,
                                                              CancellationToken cancellationToken)
        {
            CreateClientResponse createClientResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.CreateClient(createClientRequest, cancellationToken).ConfigureAwait(false);
            return createClientResponse;
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Http Status Code [{response.StatusCode}] Message [{responseBody}]</exception>
        private async Task<ClientDetails> GetClient(String clientId,
                                                    CancellationToken cancellationToken)
        {
            ClientDetails clientDetails = await this.TestingContext.DockerHelper.SecurityServiceClient.GetClient(clientId, cancellationToken).ConfigureAwait(false);
            return clientDetails;
        }

        /// <summary>
        /// Gets the clients.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Http Status Code [{response.StatusCode}] Message [{responseBody}]</exception>
        private async Task<List<ClientDetails>> GetClients(CancellationToken cancellationToken)
        {
            List<ClientDetails> clientDetailsList = await this.TestingContext.DockerHelper.SecurityServiceClient.GetClients(cancellationToken).ConfigureAwait(false);
            return clientDetailsList;
        }

        #endregion
    }
}