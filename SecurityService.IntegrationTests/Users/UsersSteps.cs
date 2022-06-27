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
    using Newtonsoft.Json;
    using Shouldly;
    using TechTalk.SpecFlow;

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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersSteps"/> class.
        /// </summary>
        /// <param name="testingContext">The testing context.</param>
        public UsersSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Givens the i create the following users.
        /// </summary>
        /// <param name="table">The table.</param>
        [Given(@"I create the following users")]
        public async Task GivenICreateTheFollowingUsers(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                // Get the claims
                Dictionary<String, String> userClaims = null;
                String claims = SpecflowTableHelper.GetStringRowValue(tableRow, "Claims");
                if (string.IsNullOrEmpty(claims) == false)
                {
                    userClaims = new Dictionary<String, String>();
                    String[] claimList = claims.Split(",");
                    foreach (String claim in claimList)
                    {
                        // Split into claim name and value
                        String[] c = claim.Split(":");
                        userClaims.Add(c[0], c[1]);
                    }
                }

                String roles = SpecflowTableHelper.GetStringRowValue(tableRow, "Roles");

                CreateUserRequest createUserRequest = new CreateUserRequest
                                                      {
                                                          EmailAddress = SpecflowTableHelper.GetStringRowValue(tableRow, "Email Address"),
                                                          FamilyName = SpecflowTableHelper.GetStringRowValue(tableRow, "Family Name"),
                                                          GivenName = SpecflowTableHelper.GetStringRowValue(tableRow, "Given Name"),
                                                          PhoneNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "Phone Number"),
                                                          MiddleName = SpecflowTableHelper.GetStringRowValue(tableRow, "Middle name"),
                                                          Claims = userClaims,
                                                          Roles = string.IsNullOrEmpty(roles) ? null : roles.Split(",").ToList(),
                                                      };
                CreateUserResponse createUserResponse = await this.CreateUser(createUserRequest, CancellationToken.None).ConfigureAwait(false);

                createUserResponse.ShouldNotBeNull();
                createUserResponse.UserId.ShouldNotBe(Guid.Empty);

                this.TestingContext.Users.Add(createUserRequest.EmailAddress, createUserResponse.UserId);
            }
        }

        /// <summary>
        /// Whens the i get the users users details are returned as follows.
        /// </summary>
        /// <param name="p0">The p0.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the users (.*) users details are returned as follows")]
        public async Task WhenIGetTheUsersUsersDetailsAreReturnedAsFollows(Int32 numberOfUsers,
                                                                           Table table)
        {
            List<UserDetails> userDetailsList = await this.GetUsers(CancellationToken.None).ConfigureAwait(false);
            userDetailsList.Count.ShouldBe(numberOfUsers);

            foreach (TableRow tableRow in table.Rows)
            {
                String emailAddress = SpecflowTableHelper.GetStringRowValue(tableRow, "Email Address");
                UserDetails userDetails = userDetailsList.SingleOrDefault(u => u.EmailAddress == emailAddress);

                userDetails.ShouldNotBeNull();

                Dictionary<String, String> userClaims = new Dictionary<String, String>();
                String claims = SpecflowTableHelper.GetStringRowValue(tableRow, "Claims");
                String[] claimList = claims.Split(",");
                foreach (String claim in claimList)
                {
                    // Split into claim name and value
                    String[] c = claim.Split(":");
                    userClaims.Add(c[0].Trim(), c[1].Trim());
                }

                String roles = SpecflowTableHelper.GetStringRowValue(tableRow, "Roles");

                userDetails.EmailAddress.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Email Address"));
                userDetails.PhoneNumber.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Phone Number"));
                userDetails.UserId.ShouldNotBe(Guid.Empty);
                userDetails.UserName.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Email Address"));
                if (string.IsNullOrEmpty(roles))
                {
                    userDetails.Roles.ShouldBeEmpty();
                }
                else
                {
                    userDetails.Roles.ShouldBe(roles.Split(",").ToList());
                }

                foreach (KeyValuePair<String, String> claim in userClaims)
                {
                    KeyValuePair<String, String> x = userDetails.Claims.Where(c => c.Key == claim.Key).SingleOrDefault();
                    x.Value.ShouldBe(claim.Value);
                }
            }
        }

        /// <summary>
        /// Whens the i get the user with user name the user details are returned as follows.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="table">The table.</param>
        [When(@"I get the user with user name '(.*)' the user details are returned as follows")]
        public async Task WhenIGetTheUserWithUserNameTheUserDetailsAreReturnedAsFollows(String userName,
                                                                                        Table table)
        {
            // Get the user id
            Guid userId = this.TestingContext.Users.Single(u => u.Key == userName).Value;

            UserDetails userDetails = await this.GetUser(userId, CancellationToken.None).ConfigureAwait(false);

            table.Rows.Count.ShouldBe(1);
            TableRow tableRow = table.Rows.First();
            userDetails.ShouldNotBeNull();

            Dictionary<String, String> userClaims = new Dictionary<String, String>();
            String claims = SpecflowTableHelper.GetStringRowValue(tableRow, "Claims");
            String[] claimList = claims.Split(",");
            foreach (String claim in claimList)
            {
                // Split into claim name and value
                String[] c = claim.Split(":");
                userClaims.Add(c[0].Trim(), c[1].Trim());
            }

            String roles = SpecflowTableHelper.GetStringRowValue(tableRow, "Roles");

            userDetails.EmailAddress.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Email Address"));
            userDetails.PhoneNumber.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Phone Number"));
            userDetails.UserId.ShouldBe(userId);
            userDetails.UserName.ShouldBe(SpecflowTableHelper.GetStringRowValue(tableRow, "Email Address"));
            if (string.IsNullOrEmpty(roles))
            {
                userDetails.Roles.ShouldBeEmpty();
            }
            else
            {
                userDetails.Roles.ShouldBe(roles.Split(",").ToList());
            }

            foreach (KeyValuePair<String, String> claim in userClaims)
            {
                KeyValuePair<String, String> x = userDetails.Claims.Where(c => c.Key == claim.Key).SingleOrDefault();
                x.Value.ShouldBe(claim.Value);
            }
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="createUserRequest">The create user request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Http Status Code [{response.StatusCode}] Message [{responseBody}]</exception>
        private async Task<CreateUserResponse> CreateUser(CreateUserRequest createUserRequest,
                                                          CancellationToken cancellationToken)
        {
            CreateUserResponse createUserResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.CreateUser(createUserRequest, cancellationToken).ConfigureAwait(false);
            return createUserResponse;
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Http Status Code [{response.StatusCode}] Message [{responseBody}]</exception>
        private async Task<UserDetails> GetUser(Guid userId,
                                                CancellationToken cancellationToken)
        {
            UserDetails userDetails = await this.TestingContext.DockerHelper.SecurityServiceClient.GetUser(userId, cancellationToken).ConfigureAwait(false);
            return userDetails;
        }

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Http Status Code [{response.StatusCode}] Message [{responseBody}]</exception>
        private async Task<List<UserDetails>> GetUsers(CancellationToken cancellationToken)
        {
            List<UserDetails> userDetailsList = await this.TestingContext.DockerHelper.SecurityServiceClient.GetUsers(String.Empty, cancellationToken).ConfigureAwait(false);
            return userDetailsList;
        }

        #endregion
    }
}