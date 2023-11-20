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
    using IntegrationTesting.Helpers;
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

        private readonly SecurityServiceSteps SecurityServiceSteps;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsSteps"/> class.
        /// </summary>
        /// <param name="testingContext">The testing context.</param>
        public ClientsSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
            this.SecurityServiceSteps = new SecurityServiceSteps(this.TestingContext.DockerHelper.SecurityServiceClient);
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
            List<CreateClientRequest> requests = table.Rows.ToCreateClientRequests();
            List<(String clientId, String secret, List<String> allowedGrantTypes)> clients = await this.SecurityServiceSteps.GivenTheFollowingClientsExist(requests);

            foreach ((String clientId, String secret, List<String> allowedGrantTypes) client in clients){
                this.TestingContext.Clients.Add(client.clientId);
            }
        }

        /// <summary>
        /// Whens the i get the clients clients details are returned as follows.
        /// </summary>
        /// <param name="numberOfClients">The number of clients.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the clients (.*) clients details are returned as follows")]
        public async Task WhenIGetTheClientsClientsDetailsAreReturnedAsFollows(Int32 numberOfClients,
                                                                               Table table){
            List<ClientDetails> expectedClientDetails = table.Rows.ToClientDetails();
            await this.SecurityServiceSteps.WhenIGetTheClientsClientsDetailsAreReturnedAsFollows(expectedClientDetails, CancellationToken.None);
        }

        /// <summary>
        /// Whens the i get the client with client identifier the client details are returned as follows.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the client with client id '(.*)' the client details are returned as follows")]
        public async Task WhenIGetTheClientWithClientIdTheClientDetailsAreReturnedAsFollows(String clientId,
                                                                                            Table table){
            List<ClientDetails> expectedClientDetails = table.Rows.ToClientDetails();
            await this.SecurityServiceSteps.WhenIGetTheClientWithClientIdTheClientDetailsAreReturnedAsFollows(expectedClientDetails, clientId, CancellationToken.None);
        }
        #endregion
    }
}