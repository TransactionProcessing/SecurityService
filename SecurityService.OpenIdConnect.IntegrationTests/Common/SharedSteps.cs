using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.OpenIdConnect.IntegrationTests.Common
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using DataTransferObjects.Requests;
    using DataTransferObjects.Responses;
    using IntegrationTesting.Helpers;
    using IntergrationTests.Common;
    using Reqnroll;
    using Shared.IntegrationTesting;
    using Shouldly;

    [Binding]
    [Scope(Tag = "shared")]
    public class SharedSteps
    {
        private readonly TestingContext TestingContext;

        private readonly SecurityServiceSteps SecurityServiceSteps;

        public SharedSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
            this.SecurityServiceSteps = new SecurityServiceSteps(this.TestingContext.DockerHelper.SecurityServiceClient);
        }

        [Given(@"I create the following roles")]
        public async Task GivenICreateTheFollowingRoles(DataTable table)
        {
            List<CreateRoleRequest> requests = table.Rows.ToCreateRoleRequests();
            List<(String, Guid)> responses = await this.SecurityServiceSteps.GivenICreateTheFollowingRoles(requests, CancellationToken.None);
            foreach ((String, Guid) response in responses)
            {
                this.TestingContext.Roles.Add(response.Item1, response.Item2);
            }
        }

        [Given(@"I create the following identity resources")]
        public async Task GivenICreateTheFollowingIdentityResources(DataTable table)
        {
            foreach (DataTableRow tableRow in table.Rows)
            {
                // Get the scopes
                String userClaims = ReqnrollTableHelper.GetStringRowValue(tableRow, "UserClaims");

                CreateIdentityResourceRequest createIdentityResourceRequest = new CreateIdentityResourceRequest
                                                                              {
                                                                                  Name = ReqnrollTableHelper
                                                                                         .GetStringRowValue(tableRow, "Name")
                                                                                         .Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N")),
                                                                                  Claims = string.IsNullOrEmpty(userClaims) ? null : userClaims.Split(",").ToList(),
                                                                                  Description = ReqnrollTableHelper.GetStringRowValue(tableRow, "Description"),
                                                                                  DisplayName = ReqnrollTableHelper.GetStringRowValue(tableRow, "DisplayName")
                                                                              };

                await this.CreateIdentityResource(createIdentityResourceRequest, CancellationToken.None).ConfigureAwait(false);
            }
        }

        [Given(@"I create the following api resources")]
        public async Task GivenICreateTheFollowingApiResources(DataTable table)
        {
            List<CreateApiResourceRequest> requests = table.Rows.ToCreateApiResourceRequests(this.TestingContext.DockerHelper.TestId);
            await this.SecurityServiceSteps.GivenTheFollowingApiResourcesExist(requests);
            foreach (CreateApiResourceRequest createApiResourceRequest in requests){
                this.TestingContext.ApiResources.Add(createApiResourceRequest.Name);
            }
        }

        private async Task CreateIdentityResource(CreateIdentityResourceRequest createIdentityResourceRequest,
                                                                             CancellationToken cancellationToken)
        {
            List<IdentityResourceDetails> identityResourceList = await this.TestingContext.DockerHelper.SecurityServiceClient.GetIdentityResources(cancellationToken);

            if (identityResourceList == null || identityResourceList.Any() == false) {
                var result = await this.TestingContext.DockerHelper.SecurityServiceClient.CreateIdentityResource(createIdentityResourceRequest, cancellationToken).ConfigureAwait(false);
                result.IsSuccess.ShouldBeTrue();

                this.TestingContext.IdentityResources.Add(createIdentityResourceRequest.Name);
            }
            else
            {
                if (identityResourceList.Any(i => i.Name == createIdentityResourceRequest.Name))
                {
                    return;
                }

                var result = await this
                                                       .TestingContext.DockerHelper.SecurityServiceClient
                                                       .CreateIdentityResource(createIdentityResourceRequest, cancellationToken)
                                                       .ConfigureAwait(false);
                result.IsSuccess.ShouldBeTrue();
                

                this.TestingContext.IdentityResources.Add(createIdentityResourceRequest.Name);
            }
        }

        [Given(@"I create the following clients")]
        public async Task GivenICreateTheFollowingClients(DataTable table)
        {
            List<CreateClientRequest> requests = table.Rows.ToCreateClientRequests(this.TestingContext.DockerHelper.TestId, this.TestingContext.DockerHelper.SecurityServiceTestUIPort);
            List<(String clientId, String secret, List<String> allowedGrantTypes)> clients = await this.SecurityServiceSteps.GivenTheFollowingClientsExist(requests);

            foreach ((String clientId, String secret, List<String> allowedGrantTypes) client in clients)
            {
                this.TestingContext.Clients.Add(client.clientId);
            }
        }

        [Given(@"I create the following users")]
        public async Task GivenICreateTheFollowingUsers(DataTable table)
        {
            List<CreateUserRequest> requests = table.Rows.ToCreateUserRequests();

            List<(String, Guid)> results = await this.SecurityServiceSteps.GivenICreateTheFollowingUsers(requests, CancellationToken.None);

            foreach ((String, Guid) response in results)
            {
                this.TestingContext.Users.Add(response.Item1, response.Item2);
            }
        }
    }
}
