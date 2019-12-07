namespace SecurityService.IntegrationTests.ApiResource
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Clients;
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
    [Scope(Tag = "apiresources")]
    public class ApiResourceSteps
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
        public ApiResourceSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Givens the i create the following API resources.
        /// </summary>
        /// <param name="table">The table.</param>
        [Given(@"I create the following api resources")]
        public async Task GivenICreateTheFollowingApiResources(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                // Get the scopes
                String scopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
                String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");

                CreateApiResourceRequest createApiResourceRequest = new CreateApiResourceRequest
                                                                    {
                                                                        Secret = SpecflowTableHelper.GetStringRowValue(tableRow, "Secret"),
                                                                        Name = SpecflowTableHelper.GetStringRowValue(tableRow, "Name"),
                                                                        Scopes = string.IsNullOrEmpty(scopes) ? null : scopes.Split(",").ToList(),
                                                                        UserClaims = string.IsNullOrEmpty(userClaims) ? null : userClaims.Split(",").ToList(),
                                                                        Description = SpecflowTableHelper.GetStringRowValue(tableRow, "Description"),
                                                                        DisplayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName")
                                                                    };
                CreateApiResourceResponse createApiResourceResponse =
                    await this.CreateApiResource(createApiResourceRequest, CancellationToken.None).ConfigureAwait(false);

                createApiResourceResponse.ShouldNotBeNull();
                createApiResourceResponse.ApiResourceName.ShouldNotBeNullOrEmpty();

                this.TestingContext.ApiResources.Add(createApiResourceResponse.ApiResourceName);
            }
        }

        /// <summary>
        /// Whens the i get the API resources API resource details are returned as follows.
        /// </summary>
        /// <param name="numberOfApiResources">The number of API resources.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the api resources (.*) api resource details are returned as follows")]
        public async Task WhenIGetTheApiResourcesApiResourceDetailsAreReturnedAsFollows(Int32 numberOfApiResources,
                                                                                        Table table)
        {
            List<ApiResourceDetails> apiResourceDetailsList = await this.GetApiResources(CancellationToken.None).ConfigureAwait(false);
            apiResourceDetailsList.Count.ShouldBe(numberOfApiResources);
            foreach (TableRow tableRow in table.Rows)
            {
                String apiResourceName = SpecflowTableHelper.GetStringRowValue(tableRow, "Name");
                ApiResourceDetails apiResourceDetails = apiResourceDetailsList.SingleOrDefault(u => u.Name == apiResourceName);

                String scopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
                String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");

                apiResourceDetails.Description.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Description"));
                apiResourceDetails.Name.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Name"));
                apiResourceDetails.DisplayName.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName"));
                if (string.IsNullOrEmpty(scopes))
                {
                    apiResourceDetails.Scopes.ShouldBeEmpty();
                }
                else
                {
                    apiResourceDetails.Scopes.ShouldBe(scopes.Split(",").ToList());
                }

                if (string.IsNullOrEmpty(userClaims))
                {
                    apiResourceDetails.UserClaims.ShouldBeEmpty();
                }
                else
                {
                    apiResourceDetails.UserClaims.ShouldBe(userClaims.Split(",").ToList());
                }
            }
        }

        /// <summary>
        /// Whens the i get the API resource with name the API resource details are returned as follows.
        /// </summary>
        /// <param name="apiResourceName">Name of the API resource.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the api resource with name '(.*)' the api resource details are returned as follows")]
        public async Task WhenIGetTheApiResourceWithNameTheApiResourceDetailsAreReturnedAsFollows(String apiResourceName,
                                                                                                  Table table)
        {
            ApiResourceDetails apiResourceDetails = await this.GetApiResource(apiResourceName, CancellationToken.None).ConfigureAwait(false);

            table.Rows.Count.ShouldBe(1);
            TableRow tableRow = table.Rows.First();
            apiResourceDetails.ShouldNotBeNull();

            String scopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
            String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");

            apiResourceDetails.Description.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Description"));
            apiResourceDetails.Name.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Name"));
            apiResourceDetails.DisplayName.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName"));
            if (string.IsNullOrEmpty(scopes))
            {
                apiResourceDetails.Scopes.ShouldBeEmpty();
            }
            else
            {
                apiResourceDetails.Scopes.ShouldBe(scopes.Split(",").ToList());
            }

            if (string.IsNullOrEmpty(userClaims))
            {
                apiResourceDetails.UserClaims.ShouldBeEmpty();
            }
            else
            {
                apiResourceDetails.UserClaims.ShouldBe(userClaims.Split(",").ToList());
            }
        }

        /// <summary>
        /// Creates the API resource.
        /// </summary>
        /// <param name="createApiResourceRequest">The create API resource request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Http Status Code [{response.StatusCode}] Message [{responseBody}]</exception>
        private async Task<CreateApiResourceResponse> CreateApiResource(CreateApiResourceRequest createApiResourceRequest,
                                                                        CancellationToken cancellationToken)
        {
            CreateApiResourceResponse createApiResourceResponse =  await this.TestingContext.DockerHelper.SecurityServiceClient.CreateApiResource(createApiResourceRequest, cancellationToken).ConfigureAwait(false);
            return createApiResourceResponse;
        }

        /// <summary>
        /// Gets the API resource.
        /// </summary>
        /// <param name="apiResourceName">Name of the API resource.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Http Status Code [{response.StatusCode}] Message [{responseBody}]</exception>
        private async Task<ApiResourceDetails> GetApiResource(String apiResourceName,
                                                              CancellationToken cancellationToken)
        {
            ApiResourceDetails apiResourceDetails = await this.TestingContext.DockerHelper.SecurityServiceClient.GetApiResource(apiResourceName, cancellationToken).ConfigureAwait(false);
            return apiResourceDetails;
        }

        /// <summary>
        /// Gets the API resources.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Http Status Code [{response.StatusCode}] Message [{responseBody}]</exception>
        private async Task<List<ApiResourceDetails>> GetApiResources(CancellationToken cancellationToken)
        {
            List<ApiResourceDetails> apiResourceDetailsList = await this.TestingContext.DockerHelper.SecurityServiceClient.GetApiResources(cancellationToken).ConfigureAwait(false);
            return apiResourceDetailsList;
        }

        #endregion
    }
}