namespace SecurityService.IntergrationTests.Users
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using DataTransferObjects;
    using DataTransferObjects.Responses;
    using IntegrationTesting.Helpers;
    using Newtonsoft.Json;
    using Reqnroll;
    using Shouldly;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
    [Scope(Tag = "users")]
    public class UsersSteps
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
        /// Initializes a new instance of the <see cref="UsersSteps"/> class.
        /// </summary>
        /// <param name="testingContext">The testing context.</param>
        public UsersSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
            this.SecurityServiceSteps = new SecurityServiceSteps(this.TestingContext.DockerHelper.SecurityServiceClient);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Givens the i create the following users.
        /// </summary>
        /// <param name="table">The table.</param>
        [Given(@"I create the following users")]
        public async Task GivenICreateTheFollowingUsers(DataTable table){
            List<CreateUserRequest> requests = table.Rows.ToCreateUserRequests();

            List<(String, Guid)> results = await this.SecurityServiceSteps.GivenICreateTheFollowingUsers(requests, CancellationToken.None);

            foreach ((String, Guid) response in results){
                this.TestingContext.Users.Add(response.Item1, response.Item2);
            }
        }

        [When(@"I get the users (.*) users details are returned as follows")]
        public async Task WhenIGetTheUsersUsersDetailsAreReturnedAsFollows(Int32 numberOfUsers,
                                                                           DataTable table){
            List<UserDetails> userDetailsList = table.Rows.ToUserDetails();
            await this.SecurityServiceSteps.WhenIGetTheUsersUsersDetailsAreReturnedAsFollows(userDetailsList, CancellationToken.None);
        }

        /// <summary>
        /// Whens the i get the user with user name the user details are returned as follows.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the user with user name '(.*)' the user details are returned as follows")]
        public async Task WhenIGetTheUserWithUserNameTheUserDetailsAreReturnedAsFollows(String userName,
                                                                                        DataTable table)
        {
            // Get the user id
            Guid userId = this.TestingContext.Users.Single(u => u.Key == userName).Value;

            List<UserDetails> userDetailsList = table.Rows.ToUserDetails();
            await this.SecurityServiceSteps.WhenIGetTheUserWithUserNameTheUserDetailsAreReturnedAsFollows(userDetailsList, userId, CancellationToken.None);
        }
        
        #endregion
    }
}