using System;
using TechTalk.SpecFlow;

namespace SecurityService.IntegrationTests.IdentityResource
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Clients;
    using DataTransferObjects.Responses;
    using IntergrationTests.Common;
    using SecurityService.DataTransferObjects.Requests;
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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityResourceSteps" /> class.
        /// </summary>
        /// <param name="testingContext">The testing context.</param>
        public IdentityResourceSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }
        #endregion

        private async Task<CreateIdentityResourceResponse> CreateIdentityResource(CreateIdentityResourceRequest createIdentityResourceRequest,
                                                                        CancellationToken cancellationToken)
        {
            CreateIdentityResourceResponse createIdentityResourceResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.CreateIdentityResource(createIdentityResourceRequest, cancellationToken).ConfigureAwait(false);
            return createIdentityResourceResponse;
        }

        [Given(@"I create the following identity resources")]
        public async Task GivenICreateTheFollowingIdentityResources(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                // Get the scopes
                String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");

                CreateIdentityResourceRequest createIdentityResourceRequest = new CreateIdentityResourceRequest
                {
                                                                        Name = SpecflowTableHelper.GetStringRowValue(tableRow, "Name"),
                                                                        Claims = string.IsNullOrEmpty(userClaims) ? null : userClaims.Split(",").ToList(),
                                                                        Description = SpecflowTableHelper.GetStringRowValue(tableRow, "Description"),
                                                                        DisplayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName")
                                                                    };
                CreateIdentityResourceResponse createIdentityResourceResponse =
                    await this.CreateIdentityResource(createIdentityResourceRequest, CancellationToken.None).ConfigureAwait(false);

                createIdentityResourceResponse.ShouldNotBeNull();
                createIdentityResourceResponse.IdentityResourceName.ShouldNotBeNullOrEmpty();

                this.TestingContext.ApiResources.Add(createIdentityResourceResponse.IdentityResourceName);
            }
        }

        private async Task<IdentityResourceDetails> GetIdentityResource(String identityResourceName,
                                                                        CancellationToken cancellationToken)
        {
            IdentityResourceDetails identityResourceDetails = await this.TestingContext.DockerHelper.SecurityServiceClient.GetIdentityResource(identityResourceName, cancellationToken).ConfigureAwait(false);
            return identityResourceDetails;
        }

        private async Task<List<IdentityResourceDetails>> GetIdentityResources(CancellationToken cancellationToken)
        {
            List<IdentityResourceDetails> identityResourceDetailsList = await this.TestingContext.DockerHelper.SecurityServiceClient.GetIdentityResources(cancellationToken).ConfigureAwait(false);
            return identityResourceDetailsList;
        }

        [When(@"I get the identity resource with name '(.*)' the identity resource details are returned as follows")]
        public async Task WhenIGetTheIdentityResourceWithNameTheIdentityResourceDetailsAreReturnedAsFollows(String identityResourceName, Table table)
        {
            IdentityResourceDetails identityResourceDetails = await this.GetIdentityResource(identityResourceName, CancellationToken.None).ConfigureAwait(false);

            table.Rows.Count.ShouldBe(1);
            TableRow tableRow = table.Rows.First();
            identityResourceDetails.ShouldNotBeNull();

            String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");

            identityResourceDetails.Description.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Description"));
            identityResourceDetails.Name.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Name"));
            identityResourceDetails.DisplayName.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName"));
            
            if (string.IsNullOrEmpty(userClaims))
            {
                identityResourceDetails.Claims.ShouldBeEmpty();
            }
            else
            {
                identityResourceDetails.Claims.ShouldBe(userClaims.Split(",").ToList());
            }
        }
        
        [When(@"I get the identity resources (.*) identity resource details are returned as follows")]
        public async Task WhenIGetTheIdentityResourcesIdentityResourceDetailsAreReturnedAsFollows(Int32 numberOfIdentityResources, Table table)
        {
            List<IdentityResourceDetails> identityResourceDetailsList = await this.GetIdentityResources(CancellationToken.None).ConfigureAwait(false);
            identityResourceDetailsList.Count.ShouldBe(numberOfIdentityResources);
            foreach (TableRow tableRow in table.Rows)
            {
                String identityResourceName = SpecflowTableHelper.GetStringRowValue(tableRow, "Name");
                IdentityResourceDetails identityResourceDetails = identityResourceDetailsList.SingleOrDefault(u => u.Name == identityResourceName);

                String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");

                identityResourceDetails.Description.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Description"));
                identityResourceDetails.Name.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Name"));
                identityResourceDetails.DisplayName.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName"));
                
                if (string.IsNullOrEmpty(userClaims))
                {
                    identityResourceDetails.Claims.ShouldBeEmpty();
                }
                else
                {
                    identityResourceDetails.Claims.ShouldBe(userClaims.Split(",").ToList());
                }
            }
        }
    }
}
