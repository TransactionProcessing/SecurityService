using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityService.OpenIdConnect.IntegrationTests.ChangePassword
{
    using IntergrationTests.Common;
    using OpenQA.Selenium;
    using SecurityService.IntegrationTests.UserLogin;
    using Shouldly;
    using TechTalk.SpecFlow;

    [Binding]
    [Scope(Tag = "changepassword")]
    public class ChangePasswordSteps
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

        public ChangePasswordSteps(TestingContext testingContext,
                                   IWebDriver webDriver)
        {
            this.TestingContext = testingContext;
            this.WebDriver = webDriver;
        }

        [Then(@"I am presented with a change password screen")]
        public async Task ThenIAmPresentedWithAChangePasswordScreen()
        {
            IWebElement changePasswordButton = await this.WebDriver.FindButton("Change Password");
            changePasswordButton.ShouldNotBeNull();
        }

        [When(@"I enter my old password '([^']*)'")]
        public async Task WhenIEnterMyOldPassword(string oldPassword)
        {
            this.WebDriver.FillIn("Input.CurrentPassword", oldPassword);
        }

        [When(@"I enter my new password '([^']*)'")]
        public async Task WhenIEnterMyNewPassword(string newPassword)
        {
            this.WebDriver.FillIn("Input.NewPassword", newPassword);
        }

        [When(@"I confirm my new password '([^']*)'")]
        public async Task WhenIConfirmMyNewPassword(string newPassword)
        {
            this.WebDriver.FillIn("Input.ConfirmPassword", newPassword);
        }

        [When(@"I click the change password button")]
        public async Task WhenIClickTheChangePasswordButton()
        {
            await this.WebDriver.ClickButton("Change Password");
        }

        [Then(@"I am returned to the application home page")]
        public void ThenIAmReturnedToTheApplicationHomePage()
        {
            this.WebDriver.Title.ShouldBe("Home Page - SecurityServiceTestUI");
        }

    }
}
