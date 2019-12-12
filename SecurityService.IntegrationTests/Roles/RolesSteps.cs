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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RolesSteps"/> class.
        /// </summary>
        /// <param name="testingContext">The testing context.</param>
        public RolesSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }

        #endregion

        [Given(@"I create the following roles")]
        public async Task GivenICreateTheFollowingRoles(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
               CreateRoleRequest createRoleRequest = new CreateRoleRequest
                {
                    RoleName = SpecflowTableHelper.GetStringRowValue(tableRow, "Role Name")
                };
                CreateRoleResponse createRoleResponse = await this.CreateRole(createRoleRequest, CancellationToken.None).ConfigureAwait(false);

                createRoleResponse.ShouldNotBeNull();
                createRoleResponse.RoleId.ShouldNotBe(Guid.Empty);

                this.TestingContext.Roles.Add(createRoleRequest.RoleName, createRoleResponse.RoleId);
            }
        }

        private async Task<CreateRoleResponse> CreateRole(CreateRoleRequest createRoleRequest,
                                                          CancellationToken cancellationToken)
        {
            CreateRoleResponse createRoleResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.CreateRole(createRoleRequest, cancellationToken).ConfigureAwait(false);
            return createRoleResponse;
        }

        [When(@"I get the role with name '(.*)' the role details are returned as follows")]
        public async Task WhenIGetTheRoleWithNameTheRoleDetailsAreReturnedAsFollows(String roleName, Table table)
        {
            // Get the role id
            Guid roleId = this.TestingContext.Roles.Single(u => u.Key == roleName).Value;

            RoleDetails roleDetails = await this.GetRole(roleId, CancellationToken.None).ConfigureAwait(false);

            table.Rows.Count.ShouldBe(1);
            TableRow tableRow = table.Rows.First();
            roleDetails.ShouldNotBeNull();

            roleDetails.RoleName.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Role Name"));
        }

        /// <summary>
        /// Gets the role.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<RoleDetails> GetRole(Guid roleId,
                                                CancellationToken cancellationToken)
        {
            RoleDetails roleDetails = await this.TestingContext.DockerHelper.SecurityServiceClient.GetRole(roleId, cancellationToken).ConfigureAwait(false);
            return roleDetails;
        }

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<List<RoleDetails>> GetRoles(CancellationToken cancellationToken)
        {
            List<RoleDetails> roleDetailsList = await this.TestingContext.DockerHelper.SecurityServiceClient.GetRoles(cancellationToken).ConfigureAwait(false);
            return roleDetailsList;
        }

        [When(@"I get the roles (.*) roles details are returned as follows")]
        public async Task WhenIGetTheRolesRolesDetailsAreReturnedAsFollows(Int32 numberOfRoles, Table table)
        {
            List<RoleDetails> roleDetailsList = await this.GetRoles(CancellationToken.None).ConfigureAwait(false);
            roleDetailsList.Count.ShouldBe(numberOfRoles);

            foreach (TableRow tableRow in table.Rows)
            {
                String roleName = SpecflowTableHelper.GetStringRowValue(tableRow, "Role Name");
                RoleDetails roleDetails = roleDetailsList.SingleOrDefault(u => u.RoleName == roleName);

                roleDetails.ShouldNotBeNull();
            }
        }


    }
}
