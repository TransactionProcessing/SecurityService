using System;

namespace SecurityService.IntegrationTests.UserLogin
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using HtmlAgilityPack;
    using IntergrationTests.Common;
    using Newtonsoft.Json;
    using OpenQA.Selenium;
    using Reqnroll;
    using Shared.IntegrationTesting;
    using Shouldly;
    using Xunit.Sdk;

    [Binding]
    [Scope(Tag = "userlogin")]
    public class UserLoginSteps
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

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoginSteps" /> class.
        /// </summary>
        /// <param name="testingContext">The testing context.</param>
        /// <param name="webDriver">The web driver.</param>
        public UserLoginSteps(TestingContext testingContext,
                              IWebDriver webDriver)
        {
            this.TestingContext = testingContext;
            this.WebDriver = webDriver;
        }

        #endregion

        [Given(@"I am on the application home page")]
        public void GivenIAmOnTheApplicationHomePage()
        {
            this.WebDriver.Navigate().GoToUrl($"https://localhost:{this.TestingContext.DockerHelper.SecurityServiceTestUIPort}");
            this.WebDriver.Title.ShouldBe("Home Page - SecurityServiceTestUI");
        }

        [When(@"I click the '(.*)' link")]
        public void WhenIClickTheLink(string linkText)
        {
            this.WebDriver.ClickLink(linkText);
        }

        [Then(@"I am presented with a login screen")]
        public async Task ThenIAmPresentedWithALoginScreen()
        {
            IWebElement loginButton = await this.WebDriver.FindButton("Login");
            loginButton.ShouldNotBeNull();
        }

        [When(@"I login with the username '([^']*)' and the provided password")]
        public void WhenILoginWithTheUsernameAndTheProvidedPassword(string userName)
        {
            this.WebDriver.FillIn("Input.Username", userName);
            this.WebDriver.FillIn("Input.Password", this.TestingContext.Password);
            this.WebDriver.ClickButton("Login");
        }



        [Then(@"I am presented with the privacy screen")]
        public async Task ThenIAmPresentedWithThePrivacyScreen()
        {
            await Retry.For(async () =>
                            {
                                var page = this.WebDriver.PageSource;

                                Console.WriteLine($"Source Is [{page}");

                                this.WebDriver.Title.ShouldBe("Privacy Policy - SecurityServiceTestUI");
                            });
            
            
        }

        [Then(@"I get an email with a confirm email address link")]
        public async Task ThenIGetAnEmailWithAConfirmEmailAddressLink()
        {
            String requestUri = $"{this.TestingContext.DockerHelper.securityServiceBaseAddressResolver("")}/api/developer/lastemail";
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await this.TestingContext.DockerHelper.httpClient.SendAsync(requestMessage, CancellationToken.None);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var emailMessage = new
                               {
                                   MessageId = Guid.Empty,
                                   Body = String.Empty
                               };
            var x = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(CancellationToken.None), emailMessage);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(x.Body);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[@href]");
            String confirmEmailAddressLink = nodes[0].GetAttributeValue("href", string.Empty);
            confirmEmailAddressLink.ShouldNotBeNullOrEmpty();

            // Cache the link
            this.TestingContext.DockerHelper.Logger.LogInformation($"ConfirmEmailAddressLink: [{confirmEmailAddressLink}]");
            this.TestingContext.ConfirmEmailAddressLink = confirmEmailAddressLink;
        }

        [Then(@"I get a welcome email with my login details")]
        public async Task ThenIGetAWelcomeEmailWithMyLoginDetails()
        {
            String requestUri = $"{this.TestingContext.DockerHelper.securityServiceBaseAddressResolver("")}/api/developer/lastemail";
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await this.TestingContext.DockerHelper.httpClient.SendAsync(requestMessage, CancellationToken.None);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var emailMessage = new
                               {
                                   MessageId = Guid.Empty,
                                   Body = String.Empty
                               };
            var x = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(CancellationToken.None), emailMessage);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(x.Body);
            var emailNode = doc.DocumentNode.SelectNodes("//*[@id='username']");
            var passwordNode = doc.DocumentNode.SelectNodes("//*[@id='password']");

            emailNode.ShouldHaveSingleItem();
            emailNode.Single().InnerText.ShouldNotBeNullOrEmpty();
            passwordNode.ShouldHaveSingleItem();
            passwordNode.Single().InnerText.ShouldNotBeNullOrEmpty();

            this.TestingContext.EmailAddress = emailNode.Single().InnerText.Trim();
            this.TestingContext.Password = passwordNode.Single().InnerText.Trim();
        }


        [When(@"I navigate to the confirm email address")]
        public void WhenINavigateToTheConfirmEmailAddress()
        {
            this.WebDriver.Navigate().GoToUrl(this.TestingContext.ConfirmEmailAddressLink);
        }

        [Then(@"I am presented with the confirm email address successful screen")]
        public void ThenIAmPresentedWithTheConfirmEmailAddressSuccessfulScreen() {
            IWebElement webElement = this.WebDriver.FindElement(By.Id("userMessage"));
            webElement.Text.ShouldBe("Thanks for confirming your email address, you should receive a welcome email soon.");
        }

    }

    public static class Extensions
    {
        public static void FillIn(this IWebDriver webDriver,
                                  String elementName,
                                  String value)
        {
            IWebElement webElement = webDriver.FindElement(By.Name(elementName));
            webElement.ShouldNotBeNull();
            webElement.SendKeys(value);
        }

        public static async Task<IWebElement> FindButton(this IWebDriver webDriver,
                                       String buttonText)
        {
            IWebElement e = null;
            await Retry.For(async () =>
                      {


                          ReadOnlyCollection<IWebElement> elements = webDriver.FindElements(By.TagName("button"));

                          var foundElements = elements.Where(e => e.GetAttribute("innerText") == buttonText).ToList();
                          foundElements.ShouldHaveSingleItem();

                          e = foundElements.Single();
                      });

            return e;
        }

        public static void ClickLink(this IWebDriver webDriver,
                                       String linkText)
        {
            IWebElement webElement = webDriver.FindElement(By.LinkText(linkText));
            webElement.ShouldNotBeNull();
            webElement.Click();
        }

        public static async Task ClickButton(this IWebDriver webDriver,
                                     String buttonText)
        {
            IWebElement webElement = await webDriver.FindButton(buttonText);
            webElement.ShouldNotBeNull();
            webElement.Click();
        }

    }
}
