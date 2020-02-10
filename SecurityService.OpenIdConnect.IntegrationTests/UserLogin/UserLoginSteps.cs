using System;
using TechTalk.SpecFlow;

namespace SecurityService.IntegrationTests.UserLogin
{
    using Coypu;
    using IntergrationTests.Common;
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
        private readonly BrowserSession BrowserSession;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoginSteps" /> class.
        /// </summary>
        /// <param name="testingContext">The testing context.</param>
        /// <param name="browserSession">The browser session.</param>
        public UserLoginSteps(TestingContext testingContext,
                              BrowserSession browserSession)
        {
            this.TestingContext = testingContext;
            this.BrowserSession = browserSession;
        }

        #endregion

        [Given(@"I am on the application home page")]
        public void GivenIAmOnTheApplicationHomePage()
        {
            this.BrowserSession.Visit($"http://localhost:{this.TestingContext.DockerHelper.SecurityServiceTestUIPort}");
            this.BrowserSession.Title.ShouldBe("Home Page - SecurityServiceTestWebClient");
        }

        [When(@"I click the '(.*)' link")]
        public void WhenIClickTheLink(string p0)
        {
            this.BrowserSession.ClickLink("Privacy", Options.FirstExact);
        }

        [Then(@"I am presented with a login screen")]
        public void ThenIAmPresentedWithALoginScreen()
        {
            ElementScope section = this.BrowserSession.FindSection("Local Login");
            section.ShouldNotBeNull();
        }

        [When(@"I login with the username '(.*)' and password '(.*)'")]
        public void WhenILoginWithTheUsernameAndPassword(String userName, String password)
        {
            this.BrowserSession.FillIn("Username").With(userName.Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N")));
            this.BrowserSession.FillIn("Password").With(password);
            this.BrowserSession.ClickButton("Login");
        }

        [Then(@"I am presented with the privacy screen")]
        public void ThenIAmPresentedWithThePrivacyScreen()
        {
            this.BrowserSession.Title.ShouldBe("Privacy Policy - SecurityServiceTestWebClient");
            //Use this page to detail your site's privacy policy.
        }


    }
}
