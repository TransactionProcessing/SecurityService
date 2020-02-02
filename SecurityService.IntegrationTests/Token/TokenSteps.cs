

namespace SecurityService.IntegrationTests.Token
{
    using TechTalk.SpecFlow;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects.Responses;
    using IntergrationTests.Common;
    using Shouldly;

    [Binding]
    [Scope(Tag = "token")]
    public class TokenSteps
    {
        private readonly TestingContext TestingContext;

        public TokenSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }

        [When(@"I request a client token with the following values")]
        public async Task WhenIRequestAClientTokenWithTheFollowingValues(Table table)
        {
            table.Rows.Count.ShouldBe(1);

            String clientId = table.Rows[0]["ClientId"];
            String clientSecret = table.Rows[0]["ClientSecret"];

            TokenResponse tokenResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.GetToken(clientId, clientSecret, CancellationToken.None);

            tokenResponse.ShouldNotBeNull();

            this.TestingContext.TokenResponse = tokenResponse;
        }

        [When(@"I request a password token with the following values")]
        public async Task WhenIRequestAPasswordTokenWithTheFollowingValues(Table table)
        {
            table.Rows.Count.ShouldBe(1);

            String clientId = table.Rows[0]["ClientId"];
            String clientSecret = table.Rows[0]["ClientSecret"];
            String username = table.Rows[0]["Username"];
            String password = table.Rows[0]["Password"];

            TokenResponse tokenResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.GetToken(username,password, clientId, clientSecret, CancellationToken.None);

            tokenResponse.ShouldNotBeNull();

            this.TestingContext.TokenResponse = tokenResponse;
        }

        [Then(@"my token is returned")]
        public void ThenMyTokenIsReturned()
        {
            this.TestingContext.TokenResponse.AccessToken.ShouldNotBeNullOrEmpty();
        }

    }
}
