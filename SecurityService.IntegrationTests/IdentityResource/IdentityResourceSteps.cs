using System;

namespace SecurityService.IntegrationTests.IdentityResource
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Clients;
    using DataTransferObjects.Responses;
    using IntergrationTests.Common;
    using Reqnroll;
    using SecurityService.DataTransferObjects.Requests;
    using SecurityService.IntegrationTesting.Helpers;
    using Shouldly;

    [Binding]
    [Scope(Tag = "identityresources")]
    public class IdentityResourceSteps
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
        /// Initializes a new instance of the <see cref="IdentityResourceSteps" /> class.
        /// </summary>
        /// <param name="testingContext">The testing context.</param>
        public IdentityResourceSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
            this.SecurityServiceSteps = new SecurityServiceSteps(this.TestingContext.DockerHelper.SecurityServiceClient);
        }
        #endregion
        
        [Given(@"I create the following identity resources")]
        public async Task GivenICreateTheFollowingIdentityResources(DataTable table){
            List<CreateIdentityResourceRequest> requests = table.Rows.ToCreateIdentityResourceRequest();
            List<CreateIdentityResourceResponse> responses = await this.SecurityServiceSteps.GivenICreateTheFollowingIdentityResources(requests, CancellationToken.None);

            foreach (CreateIdentityResourceResponse response in responses){
                this.TestingContext.ApiResources.Add(response.IdentityResourceName);
            }
        }
        
        [When(@"I get the identity resource with name '(.*)' the identity resource details are returned as follows")]
        public async Task WhenIGetTheIdentityResourceWithNameTheIdentityResourceDetailsAreReturnedAsFollows(String identityResourceName, DataTable table)
        {
            List<IdentityResourceDetails> expectedIdentityResourceDetailsList = table.Rows.ToIdentityResourceDetails();
            await this.SecurityServiceSteps.WhenIGetTheIdentityResourceWithNameTheIdentityResourceDetailsAreReturnedAsFollows(expectedIdentityResourceDetailsList, identityResourceName, CancellationToken.None);
        }
        
        [When(@"I get the identity resources (.*) identity resource details are returned as follows")]
        public async Task WhenIGetTheIdentityResourcesIdentityResourceDetailsAreReturnedAsFollows(Int32 numberOfIdentityResources, DataTable table)
        {
            List<IdentityResourceDetails> expectedIdentityResourceDetailsList = table.Rows.ToIdentityResourceDetails();
            await this.SecurityServiceSteps.WhenIGetTheIdentityResourcesIdentityResourceDetailsAreReturnedAsFollows(expectedIdentityResourceDetailsList, CancellationToken.None);
        }
    }
}