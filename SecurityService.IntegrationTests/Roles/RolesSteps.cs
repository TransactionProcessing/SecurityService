using System;
using TechTalk.SpecFlow;

namespace SecurityService.IntegrationTests.Roles
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects.Requests;
    using DataTransferObjects.Responses;
    using IntegrationTesting.Helpers;
    using IntergrationTests.Common;
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
        public async Task GivenICreateTheFollowingRoles(Table table){
            List<CreateRoleRequest> requests = table.Rows.ToCreateRoleRequests();
            List<(String, Guid)> responses = await this.SecurityServiceSteps.GivenICreateTheFollowingRoles(requests, CancellationToken.None);
            foreach ((String, Guid) response in responses){
                this.TestingContext.Roles.Add(response.Item1, response.Item2);
            }
        }

        [When(@"I get the role with name '(.*)' the role details are returned as follows")]
        public async Task WhenIGetTheRoleWithNameTheRoleDetailsAreReturnedAsFollows(String roleName, Table table)
        {
            List<RoleDetails> requests = table.Rows.ToRoleDetails();
            // Get the role id
            Guid roleId = this.TestingContext.Roles.Single(u => u.Key == roleName).Value;
            await this.SecurityServiceSteps.WhenIGetTheRoleWithNameTheRoleDetailsAreReturnedAsFollows(requests, roleId, CancellationToken.None);
        }
        
        [When(@"I get the roles (.*) roles details are returned as follows")]
        public async Task WhenIGetTheRolesRolesDetailsAreReturnedAsFollows(Int32 numberOfRoles, Table table)
        {
            List<RoleDetails> requests = table.Rows.ToRoleDetails();
            await this.SecurityServiceSteps.WhenIGetTheRolesRolesDetailsAreReturnedAsFollows(requests, CancellationToken.None);
        }

    }
}
