using System;
using TechTalk.SpecFlow;

namespace SecurityService.IntergrationTests.Users
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using SecurityService.DataTransferObjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using DataTransferObjects.Responses;
    using Newtonsoft.Json;
    using Shouldly;

    [Binding]
    [Scope(Tag = "users")]
    public class UsersSteps
    {
        private readonly TestingContext TestingContext;

        public UsersSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }

        [Given(@"I create the following users")]
        public async Task GivenICreateTheFollowingUsers(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                // Get the claims
                Dictionary<String, String> userClaims = null;
                String claims = SpecflowTableHelper.GetStringRowValue(tableRow, "Claims");
                if (String.IsNullOrEmpty(claims) == false)
                {
                    userClaims = new Dictionary<String, String>();
                    var claimList = claims.Split(",");
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
                                                          Roles = String.IsNullOrEmpty(roles) ? null : roles.Split(",").ToList(),
                                                          Password = SpecflowTableHelper.GetStringRowValue(tableRow, "Password"),

                };
                CreateUserResponse createUserResponse = await this.CreateUser(createUserRequest, CancellationToken.None).ConfigureAwait(false);

                createUserResponse.ShouldNotBeNull();
                createUserResponse.UserId.ShouldNotBe(Guid.Empty);

                this.TestingContext.Users.Add(createUserRequest.EmailAddress, createUserResponse.UserId);

            }
        }

        [When(@"I get the user with user name '(.*)' the user details are returned as follows")]
        public async Task WhenIGetTheUserWithUserNameTheUserDetailsAreReturnedAsFollows(String userName, Table table)
        {
            // Get the user id
            Guid userId = this.TestingContext.Users.Single(u => u.Key == userName).Value;

            UserDetails userDetails = await this.GetUser(userId, CancellationToken.None).ConfigureAwait(false);

            table.Rows.Count.ShouldBe(1);
            var tableRow = table.Rows.First();
            userDetails.ShouldNotBeNull();

            Dictionary<String, String> userClaims = new Dictionary<String, String>();
            String claims = SpecflowTableHelper.GetStringRowValue(tableRow, "Claims");
            var claimList = claims.Split(",");
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
            if (String.IsNullOrEmpty(roles))
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

        [When(@"I get the users (.*) users details are returned as follows")]
        public async Task WhenIGetTheUsersUsersDetailsAreReturnedAsFollows(int p0, Table table)
        {
            var userDetailsList = await this.GetUsers(CancellationToken.None).ConfigureAwait(false);

            foreach (TableRow tableRow in table.Rows)
            {
                var emailAddress = SpecflowTableHelper.GetStringRowValue(tableRow, "Email Address");
                var userDetails = userDetailsList.SingleOrDefault(u => u.EmailAddress == emailAddress);

                userDetails.ShouldNotBeNull();

                Dictionary<String, String> userClaims = new Dictionary<String, String>();
                String claims = SpecflowTableHelper.GetStringRowValue(tableRow, "Claims");
                var claimList = claims.Split(",");
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
                if (String.IsNullOrEmpty(roles))
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
        
        private async Task<UserDetails> GetUser(Guid userId,
                             CancellationToken cancellationToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri($"http://127.0.0.1:{this.TestingContext.DockerHelper.SecurityServicePort}");

                HttpResponseMessage response = await client.GetAsync($"api/users/{userId}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    String responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UserDetails>(responseBody);
                }
                else
                {
                    String responseBody = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Http Status Code [{response.StatusCode}] Message [{responseBody}]");
                }
            }
        }

        private async Task<List<UserDetails>> GetUsers(CancellationToken cancellationToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri($"http://127.0.0.1:{this.TestingContext.DockerHelper.SecurityServicePort}");

                Console.Out.WriteLine($"GET Uri is [{client.BaseAddress}api/users]");

                HttpResponseMessage response = await client.GetAsync($"api/users", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    String responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<UserDetails>>(responseBody);
                }
                else
                {
                    String responseBody = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Http Status Code [{response.StatusCode}] Message [{responseBody}]");
                }
            }
        }

        private async Task<CreateUserResponse> CreateUser(CreateUserRequest createUserRequest,
                                            CancellationToken cancellationToken)
        {
            String requestSerialised = JsonConvert.SerializeObject(createUserRequest);
            StringContent content = new StringContent(requestSerialised, Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri($"http://127.0.0.1:{this.TestingContext.DockerHelper.SecurityServicePort}");

                Console.Out.WriteLine($"POST Uri is [{client.BaseAddress}api/users]");

                HttpResponseMessage response = await client.PostAsync("api/users", content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    String responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<CreateUserResponse>(responseBody);
                }
                else
                {
                    String responseBody = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Http Status Code [{response.StatusCode}] Message [{responseBody}]");
                }
            }

            return null;
        }
    }
}
