using System;
using TechTalk.SpecFlow;

namespace SecurityService.IntegrationTests.UserLogin
{
    using IntergrationTests.Common;
    using OpenQA.Selenium;
    using Shouldly;

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
        /// <param name="browserSession">The browser session.</param>
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
            this.WebDriver.Navigate().GoToUrl($"http://localhost:{this.TestingContext.DockerHelper.SecurityServiceTestUIPort}");
            //this.BrowserSession.Visit($"http://localhost:{this.TestingContext.DockerHelper.SecurityServiceTestUIPort}");
            this.WebDriver.Title.ShouldBe("Home Page - SecurityServiceTestWebClient");
            //this.BrowserSession.Title.ShouldBe("Home Page - SecurityServiceTestWebClient");

        }

        [When(@"I click the '(.*)' link")]
        public void WhenIClickTheLink(string linkText)
        {
            this.WebDriver.ClickButton("Privacy");
            //this.BrowserSession.ClickLink("Privacy", Options.FirstExact);
        }

        [Then(@"I am presented with a login screen")]
        public void ThenIAmPresentedWithALoginScreen()
        {
            IWebElement loginButton = this.WebDriver.FindElement(By.LinkText("Login"));
            loginButton.ShouldNotBeNull();
        }

        [When(@"I login with the username '(.*)' and password '(.*)'")]
        public void WhenILoginWithTheUsernameAndPassword(String userName, String password)
        {
            //var userNameTextBox = this.WebDriver.FindElement(By.Name("Username"));
            //userNameTextBox.ShouldNotBeNull();
            //userNameTextBox.SendKeys(userName);
            this.WebDriver.FillIn("Username", userName.Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N")));
            this.WebDriver.FillIn("Password", password);
            this.WebDriver.ClickButton("Login");
            //this.BrowserSession.FillIn("Username").With(userName.Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N")));
            //this.BrowserSession.FillIn("Password").With(password);
            //this.BrowserSession.ClickButton("Login");
        }

        [Then(@"I am presented with the privacy screen")]
        public void ThenIAmPresentedWithThePrivacyScreen()
        {
            this.WebDriver.Title.ShouldBe("Privacy Policy - SecurityServiceTestWebClient");
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

        public static void ClickButton(this IWebDriver webDriver,
                                       String buttonText)
        {
            IWebElement webElement = webDriver.FindElement(By.LinkText(buttonText));
            webElement.ShouldNotBeNull();
            webElement.Click();
        }
    }
}
