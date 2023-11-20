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
    using IntegrationTesting.Helpers;
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

        private readonly SecurityServiceSteps SecurityServiceSteps;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsSteps" /> class.
        /// </summary>
        /// <param name="testingContext">The testing context.</param>
        public ApiResourceSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
            this.SecurityServiceSteps = new SecurityServiceSteps(this.TestingContext.DockerHelper.SecurityServiceClient);
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
            List<CreateApiResourceRequest> requests = table.Rows.ToCreateApiResourceRequests();
            await this.SecurityServiceSteps.GivenTheFollowingApiResourcesExist(requests);
        }

        /// <summary>
        /// Whens the i get the API resources API resource details are returned as follows.
        /// </summary>
        /// <param name="numberOfApiResources">The number of API resources.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the api resources (.*) api resource details are returned as follows")]
        public async Task WhenIGetTheApiResourcesApiResourceDetailsAreReturnedAsFollows(Int32 numberOfApiResources,
                                                                                        Table table){
            List<ApiResourceDetails> expectedDetails = table.Rows.ToApiResourceDetails();
            await this.SecurityServiceSteps.WhenIGetTheApiResourcesApiResourceDetailsAreReturnedAsFollows(expectedDetails, CancellationToken.None);
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
            List<ApiResourceDetails> expectedDetails = table.Rows.ToApiResourceDetails();
            await this.SecurityServiceSteps.WhenIGetTheApiResourceWithNameTheApiResourceDetailsAreReturnedAsFollows(expectedDetails, apiResourceName, CancellationToken.None);
        }
        
   


        #endregion
    }
}