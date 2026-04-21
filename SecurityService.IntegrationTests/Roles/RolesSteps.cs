using System;
using SecurityService.DataTransferObjects;

namespace SecurityService.IntegrationTests.Roles
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using IntegrationTesting.Helpers;
    using IntergrationTests.Common;
    using Reqnroll;
    using Shouldly;

    [Binding]
    [Scope(Tag = "roles")]
    public class RolesSteps
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
        /// Initializes a new instance of the <see cref="RolesSteps"/> class.
        /// </summary>
        /// <param name="testingContext">The testing context.</param>
        public RolesSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
            this.SecurityServiceSteps = new SecurityServiceSteps(this.TestingContext.DockerHelper.SecurityServiceClient);
        }

        #endregion

        [Given(@"I create the following roles")]
        public async Task GivenICreateTheFollowingRoles(DataTable table){
            List<CreateRoleRequest> requests = table.Rows.ToCreateRoleRequests();
            List<(String, String)> responses = await this.SecurityServiceSteps.GivenICreateTheFollowingRoles(requests, CancellationToken.None);
            foreach ((String, String) response in responses){
                this.TestingContext.Roles.Add(response.Item1, response.Item2);
            }
        }

        [When(@"I get the role with name '(.*)' the role details are returned as follows")]
        public async Task WhenIGetTheRoleWithNameTheRoleDetailsAreReturnedAsFollows(String roleName, DataTable table)
        {
            List<RoleResponse> requests = table.Rows.ToRoleResponses();
            // Get the role id
            String roleId = this.TestingContext.Roles.Single(u => u.Key == roleName).Value;
            await this.SecurityServiceSteps.WhenIGetTheRoleWithNameTheRoleDetailsAreReturnedAsFollows(requests, roleId, CancellationToken.None);
        }
        
        [When(@"I get the roles (.*) roles details are returned as follows")]
        public async Task WhenIGetTheRolesRolesDetailsAreReturnedAsFollows(Int32 numberOfRoles, Table table)
        {
            List<RoleResponse> requests = table.Rows.ToRoleResponses();
            await this.SecurityServiceSteps.WhenIGetTheRolesRolesDetailsAreReturnedAsFollows(requests, CancellationToken.None);
        }

    }
}
