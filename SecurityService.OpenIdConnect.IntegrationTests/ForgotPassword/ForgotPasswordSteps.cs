using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityService.OpenIdConnect.IntegrationTests.ForgotPassword
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using HtmlAgilityPack;
    using IntergrationTests.Common;
    using Newtonsoft.Json;
    using OpenQA.Selenium;
    using SecurityService.IntegrationTests.UserLogin;
    using Shared.IntegrationTesting;
    using Shouldly;
    using TechTalk.SpecFlow;

    [Binding]
    [Scope(Tag = "forgotpassword")]
    public class ForgotPasswordSteps
    {
        #region Fields

        /// <summary>
        /// The testing context
        /// </summary>
        private readonly TestingContext TestingContext;

        /// <summary>
        /// The browser session
        /// </summary>
        private readonly IWebDriver WebDriver;

        #endregion

        public ForgotPasswordSteps(TestingContext testingContext,
                                   IWebDriver webDriver)
        {
            this.TestingContext = testingContext;
            this.WebDriver = webDriver;
        }

        [When(@"I click on the Forgot Password Button")]
        public async Task WhenIClickOnTheForgotPasswordButton()
        {
            await this.WebDriver.ClickButton("Forgot Password");
        }

        [Then(@"I am presented with the forgot password screen")]
        public async Task ThenIAmPresentedWithTheForgotPasswordScreen()
        {
            IWebElement resetPasswordButton = await this.WebDriver.FindButton("Reset Password");
            resetPasswordButton.ShouldNotBeNull();
        }

        [When(@"I enter my username '([^']*)'")]
        public void WhenIEnterMyUsername(string userName)
        {
            this.WebDriver.FillIn("Input.Username", userName.Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N")));
        }

        [When(@"I enter my email address '([^']*)'")]
        public void WhenIEnterMyEmailAddress(string emailAddress)
        {
            this.WebDriver.FillIn("Input.EmailAddress", emailAddress.Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N")));
        }

        [When(@"I click on the Reset Password button")]
        public async Task WhenIClickOnTheResetPasswordButton()
        {
            await this.WebDriver.ClickButton("Reset Password");
        }

        [Then(@"I get an email with a forgot password link")]
        public async Task ThenIGetAnEmailWithAForgotPasswordLink() {
            String requestUri = $"{this.TestingContext.DockerHelper.securityServiceBaseAddressResolver("")}/api/developer/lastemail";
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await this.TestingContext.DockerHelper.httpClient.SendAsync(requestMessage, CancellationToken.None);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var emailMessage = new {
                                       MessageId = Guid.Empty,
                                       Body = String.Empty
                                   };
            var x = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(CancellationToken.None), emailMessage);

            var doc = new HtmlDocument();
            doc.LoadHtml(x.Body);
            var nodes = doc.DocumentNode.SelectNodes("//a[@href]");
            var resetPasswordLink = nodes[0].GetAttributeValue("href", string.Empty);
            resetPasswordLink.ShouldNotBeNullOrEmpty();

            // Cache the link
            this.TestingContext.ResetPasswordLink = resetPasswordLink;
        }

        [When(@"I navigate to the forgot password link")]
        public void WhenINavigateToTheForgotPasswordLink()
        {
            this.WebDriver.Navigate().GoToUrl(this.TestingContext.ResetPasswordLink);
        }

        [Then(@"I am presented with the reset password screen")]
        public async Task ThenIAmPresentedWithTheResetPasswordScreen()
        {
            IWebElement resetPasswordButton = await this.WebDriver.FindButton("Confirm Reset Password");
            resetPasswordButton.ShouldNotBeNull();
        }

        [When(@"I enter my new password '([^']*)'")]
        public void WhenIEnterMyNewPassword(string newpassword)
        {
            this.WebDriver.FillIn("Input.Password", newpassword);
        }

        [When(@"I confirm my new password '([^']*)'")]
        public void WhenIConfirmMyNewPassword(string confirmpassword)
        {
            this.WebDriver.FillIn("Input.ConfirmPassword", confirmpassword);
        }

        [When(@"I click the reset password button")]
        public async Task WhenIClickTheResetPasswordButton()
        {
            await this.WebDriver.ClickButton("Confirm Reset Password");
        }

        [Then(@"my password is reset successfully")]
        public async Task ThenMyPasswordIsResetSuccessfully()
        {
            await Retry.For(async () =>
                            {
                                var page = this.WebDriver.PageSource;

                                Console.WriteLine($"Source Is [{page}");

                                this.WebDriver.Title.ShouldBe("Home Page - SecurityServiceTestUI");
                            });
        }

    }
}
