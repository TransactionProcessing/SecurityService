using System;
using TechTalk.SpecFlow;

namespace SecurityService.IntegrationTests.UserLogin
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using IntergrationTests.Common;
    using OpenQA.Selenium;
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
            this.WebDriver.ClickLink("Privacy");
        }

        [Then(@"I am presented with a login screen")]
        public void ThenIAmPresentedWithALoginScreen()
        {
            IWebElement loginButton = this.WebDriver.FindButton("Login");
            loginButton.ShouldNotBeNull();
        }

        [When(@"I login with the username '(.*)' and password '(.*)'")]
        public void WhenILoginWithTheUsernameAndPassword(String userName, String password)
        {
            this.WebDriver.FillIn("Username", userName.Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N")));
            this.WebDriver.FillIn("Password", password);
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

        public static IWebElement FindButton(this IWebDriver webDriver,
                                       String buttonText)
        {
            ReadOnlyCollection<IWebElement> elements = webDriver.FindElements(By.TagName("button"));

            List<IWebElement> e = elements.Where(e => e.GetAttribute("innerText") == buttonText).ToList();
            e.ShouldHaveSingleItem();

            return e.Single();
        }

        public static void ClickLink(this IWebDriver webDriver,
                                       String linkText)
        {
            IWebElement webElement = webDriver.FindElement(By.LinkText(linkText));
            webElement.ShouldNotBeNull();
            webElement.Click();
        }

        public static void ClickButton(this IWebDriver webDriver,
                                     String buttonText)
        {
            IWebElement webElement = webDriver.FindButton(buttonText);
            webElement.ShouldNotBeNull();
            webElement.Click();
        }
    }
}
