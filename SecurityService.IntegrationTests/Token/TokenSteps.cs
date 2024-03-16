

namespace SecurityService.IntegrationTests.Token
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects.Responses;
    using IntergrationTests.Common;
    using Reqnroll;
    using Shouldly;
    using SecurityService.IntegrationTesting.Helpers;

    [Binding]
    [Scope(Tag = "token")]
    public class TokenSteps
    {
        private readonly TestingContext TestingContext;

        private readonly SecurityServiceSteps SecurityServiceSteps;

        public TokenSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
            this.SecurityServiceSteps = new SecurityServiceSteps(this.TestingContext.DockerHelper.SecurityServiceClient);
        }

        [When(@"I request a client token with the following values")]
        public async Task WhenIRequestAClientTokenWithTheFollowingValues(Table table)
        {
            table.Rows.Count.ShouldBe(1);

            String clientId = table.Rows[0]["ClientId"];
            String clientSecret = table.Rows[0]["ClientSecret"];

            String tokenResponse = await this.SecurityServiceSteps.GetClientToken(clientId, clientSecret, CancellationToken.None);

            this.TestingContext.AccessToken = tokenResponse;
        }

        [When(@"I request a password token with the following values")]
        public async Task WhenIRequestAPasswordTokenWithTheFollowingValues(Table table)
        {
            table.Rows.Count.ShouldBe(1);

            String clientId = table.Rows[0]["ClientId"];
            String clientSecret = table.Rows[0]["ClientSecret"];
            String username = table.Rows[0]["Username"];
            String password = table.Rows[0]["Password"];

            String tokenResponse = await this.SecurityServiceSteps.GetPasswordToken(clientId, clientSecret, username, password,CancellationToken.None);

            this.TestingContext.AccessToken = tokenResponse;
        }

        [Then(@"my token is returned")]
        public void ThenMyTokenIsReturned()
        {
            this.TestingContext.AccessToken.ShouldNotBeNullOrEmpty();
        }

    }
}
