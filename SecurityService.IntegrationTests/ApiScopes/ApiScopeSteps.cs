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

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsSteps" /> class.
        /// </summary>
        /// <param name="testingContext">The testing context.</param>
        public ApiScopeSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }

        #endregion

        /// <summary>
        /// Givens the i create the following API scopes.
        /// </summary>
        /// <param name="table">The table.</param>
        [Given(@"I create the following api scopes")]
        public async Task GivenICreateTheFollowingApiScopes(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                CreateApiScopeRequest createApiScopeRequest = new CreateApiScopeRequest
                                                              {
                                                                  Name = SpecflowTableHelper.GetStringRowValue(tableRow, "Name"),
                                                                  Description = SpecflowTableHelper.GetStringRowValue(tableRow, "Description"),
                                                                  DisplayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName")
                                                              };
                var createApiScopeResponse =
                    await this.CreateApiScope(createApiScopeRequest, CancellationToken.None).ConfigureAwait(false);

                createApiScopeResponse.ShouldNotBeNull();
                createApiScopeResponse.ApiScopeName.ShouldNotBeNullOrEmpty();

                this.TestingContext.ApiScopes.Add(createApiScopeResponse.ApiScopeName);
            }
        }

        /// <summary>
        /// Whens the i get the API scope with name the API scope details are returned as follows.
        /// </summary>
        /// <param name="apiScopeName">Name of the API scope.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the api scope with name '(.*)' the api scope details are returned as follows")]
        public async Task WhenIGetTheApiScopeWithNameTheApiScopeDetailsAreReturnedAsFollows(String apiScopeName, Table table)
        {
            ApiScopeDetails apiScopeDetails = await this.GetApiScope(apiScopeName, CancellationToken.None).ConfigureAwait(false);

            table.Rows.Count.ShouldBe(1);
            TableRow tableRow = table.Rows.First();
            apiScopeDetails.ShouldNotBeNull();
            
            apiScopeDetails.Description.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Description"));
            apiScopeDetails.Name.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Name"));
            apiScopeDetails.DisplayName.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName"));
        }

        /// <summary>
        /// Whens the i get the API scopes API scope details are returned as follows.
        /// </summary>
        /// <param name="numberOfApiScopes">The number of API scopes.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the api scopes (.*) api scope details are returned as follows")]
        public async Task WhenIGetTheApiScopesApiScopeDetailsAreReturnedAsFollows(Int32 numberOfApiScopes, Table table)
        {
            List<ApiScopeDetails> apiScopeDetailsList = await this.GetApiScopes(CancellationToken.None).ConfigureAwait(false);
            apiScopeDetailsList.Count.ShouldBe(numberOfApiScopes);
            foreach (TableRow tableRow in table.Rows)
            {
                String apiScopeName = SpecflowTableHelper.GetStringRowValue(tableRow, "Name");
                ApiScopeDetails? apiResourceDetails = apiScopeDetailsList.SingleOrDefault(u => u.Name == apiScopeName);

                apiResourceDetails.Description.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Description"));
                apiResourceDetails.Name.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Name"));
                apiResourceDetails.DisplayName.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName"));
            }
        }

        /// <summary>
        /// Creates the API scope.
        /// </summary>
        /// <param name="createApiScopeRequest">The create API scope request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<CreateApiScopeResponse> CreateApiScope(CreateApiScopeRequest createApiScopeRequest,
                                                                  CancellationToken cancellationToken)
        {
            CreateApiScopeResponse createApiScopeResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.CreateApiScope(createApiScopeRequest, cancellationToken).ConfigureAwait(false);
            return createApiScopeResponse;
        }

        /// <summary>
        /// Gets the API scope.
        /// </summary>
        /// <param name="apiScopeName">Name of the API scope.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<ApiScopeDetails> GetApiScope(String apiScopeName,
                                                              CancellationToken cancellationToken)
        {
            ApiScopeDetails apiScopeDetails = await this.TestingContext.DockerHelper.SecurityServiceClient.GetApiScope(apiScopeName, cancellationToken).ConfigureAwait(false);
            return apiScopeDetails;
        }

        /// <summary>
        /// Gets the API scopes.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<List<ApiScopeDetails>> GetApiScopes(CancellationToken cancellationToken)
        {
            List<ApiScopeDetails> apiScopeDetailsList = await this.TestingContext.DockerHelper.SecurityServiceClient.GetApiScopes(cancellationToken).ConfigureAwait(false);
            return apiScopeDetailsList;
        }
    }
}
