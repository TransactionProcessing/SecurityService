using System;
using TechTalk.SpecFlow;

namespace SecurityService.IntegrationTests.ApiScopes
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Clients;
    using DataTransferObjects.Requests;
    using DataTransferObjects.Responses;
    using IntegrationTesting.Helpers;
    using IntergrationTests.Common;
    using Shouldly;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
    public class ApiScopeSteps
    {
        #region Fields

        /// <summary>
        /// The testing context
        /// </summary>
        private readonly TestingContext TestingContext;

        #endregion

        private readonly SecurityServiceSteps SecurityServiceSteps;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsSteps" /> class.
        /// </summary>
        /// <param name="testingContext">The testing context.</param>
        public ApiScopeSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
            this.SecurityServiceSteps = new SecurityServiceSteps(this.TestingContext.DockerHelper.SecurityServiceClient);
        }

        #endregion

        /// <summary>
        /// Givens the i create the following API scopes.
        /// </summary>
        /// <param name="table">The table.</param>
        [Given(@"I create the following api scopes")]
        public async Task GivenICreateTheFollowingApiScopes(Table table)
        {
            List<CreateApiScopeRequest> requests = table.Rows.ToCreateApiScopeRequests();
            await this.SecurityServiceSteps.GivenICreateTheFollowingApiScopes(requests);
        }

        /// <summary>
        /// Whens the i get the API scope with name the API scope details are returned as follows.
        /// </summary>
        /// <param name="apiScopeName">Name of the API scope.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the api scope with name '(.*)' the api scope details are returned as follows")]
        public async Task WhenIGetTheApiScopeWithNameTheApiScopeDetailsAreReturnedAsFollows(String apiScopeName, Table table)
        {
            List<ApiScopeDetails> expectedDetails = table.Rows.ToApiScopeDetails();
            await this.SecurityServiceSteps.WhenIGetTheApiScopeWithNameTheApiScopeDetailsAreReturnedAsFollows(expectedDetails,apiScopeName, CancellationToken.None);
            
        }

        /// <summary>
        /// Whens the i get the API scopes API scope details are returned as follows.
        /// </summary>
        /// <param name="numberOfApiScopes">The number of API scopes.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the api scopes (.*) api scope details are returned as follows")]
        public async Task WhenIGetTheApiScopesApiScopeDetailsAreReturnedAsFollows(Int32 numberOfApiScopes, Table table)
        {
            List<ApiScopeDetails> expectedDetails = table.Rows.ToApiScopeDetails();
            await this.SecurityServiceSteps.WhenIGetTheApiScopesApiScopeDetailsAreReturnedAsFollows(expectedDetails, CancellationToken.None);
        }
    }
}
