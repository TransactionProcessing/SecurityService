﻿namespace SecurityService.IntegrationTests.Clients
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
    using Newtonsoft.Json;
    using Shouldly;
    using TechTalk.SpecFlow;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
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
                String grantTypes = SpecflowTableHelper.GetStringRowValue(tableRow, "GrantTypes");

                CreateClientRequest createClientRequest = new CreateClientRequest
                                                          {
                                                              ClientId = SpecflowTableHelper.GetStringRowValue(tableRow, "ClientId"),
                                                              Secret = SpecflowTableHelper.GetStringRowValue(tableRow, "Secret"),
                                                              ClientName = SpecflowTableHelper.GetStringRowValue(tableRow, "Name"),
                                                              AllowedScopes = string.IsNullOrEmpty(scopes) ? null : scopes.Split(",").ToList(),
                                                              AllowedGrantTypes = string.IsNullOrEmpty(grantTypes) ? null : grantTypes.Split(",").ToList(),
                                                              ClientDescription = SpecflowTableHelper.GetStringRowValue(tableRow, "Description")
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
            String requestSerialised = JsonConvert.SerializeObject(createClientRequest);
            StringContent content = new StringContent(requestSerialised, Encoding.UTF8, "application/json");

            using(HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri($"http://127.0.0.1:{this.TestingContext.DockerHelper.SecurityServicePort}");

                Console.Out.WriteLine($"POST Uri is [{client.BaseAddress}api/clients]");

                HttpResponseMessage response = await client.PostAsync("api/clients", content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    String responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<CreateClientResponse>(responseBody);
                }
                else
                {
                    String responseBody = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Http Status Code [{response.StatusCode}] Message [{responseBody}]");
                }
            }

            return null;
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
            using(HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri($"http://127.0.0.1:{this.TestingContext.DockerHelper.SecurityServicePort}");

                HttpResponseMessage response = await client.GetAsync($"api/clients/{clientId}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    String responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ClientDetails>(responseBody);
                }
                else
                {
                    String responseBody = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Http Status Code [{response.StatusCode}] Message [{responseBody}]");
                }
            }
        }

        /// <summary>
        /// Gets the clients.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Http Status Code [{response.StatusCode}] Message [{responseBody}]</exception>
        private async Task<List<ClientDetails>> GetClients(CancellationToken cancellationToken)
        {
            using(HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri($"http://127.0.0.1:{this.TestingContext.DockerHelper.SecurityServicePort}");

                HttpResponseMessage response = await client.GetAsync("api/clients", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    String responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<ClientDetails>>(responseBody);
                }
                else
                {
                    String responseBody = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Http Status Code [{response.StatusCode}] Message [{responseBody}]");
                }
            }
        }

        #endregion
    }
}