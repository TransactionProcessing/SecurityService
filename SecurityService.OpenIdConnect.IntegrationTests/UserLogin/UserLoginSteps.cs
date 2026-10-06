using System;
using Shared.Serialisation;

namespace SecurityService.IntegrationTests.UserLogin
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using HtmlAgilityPack;
    using IntergrationTests.Common;
    using OpenQA.Selenium;
    using Reqnroll;
    using Shared.IntegrationTesting;
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
        /// <param name="webDriver">The web driver.</param>
        public UserLoginSteps(TestingContext testingContext,
                              IWebDriver webDriver)
        {
            this.TestingContext = testingContext;
            this.WebDriver = webDriver;
        }

        #endregion

        [Given(@"I am on the application home page")]
        public async Task GivenIAmOnTheApplicationHomePage()
        {
            this.WebDriver.Navigate().GoToUrl($"https://localhost:{this.TestingContext.DockerHelper.SecurityServiceTestUIPort}");
            await Retry.For(async () =>
                            {
                                this.WebDriver.Title.ShouldBe("Home Page - SecurityServiceTestUI");
                            });
        }

        [When(@"I click the '(.*)' link")]
        public async Task WhenIClickTheLink(string linkText)
        {
            await this.WebDriver.ClickLink(linkText);
        }

        [Then(@"I am presented with a login screen")]
        public async Task ThenIAmPresentedWithALoginScreen()
        {
            try
            {
                await Retry.For(async () =>
                                {
                                    IWebElement loginButton = await this.WebDriver.FindButton("Login");
                                    loginButton.ShouldNotBeNull();
                                });
            }
            catch
            {
                string path = "<unavailable>";
                string queryKeys = "<unavailable>";
                string title = "<unavailable>";
                string bodyText = "<unavailable>";

                try
                {
                    if (Uri.TryCreate(this.WebDriver.Url, UriKind.Absolute, out Uri currentUri))
                    {
                        path = currentUri.AbsolutePath;
                        queryKeys = String.Join(",", currentUri.Query
                                                                   .TrimStart('?')
                                                                   .Split('&', StringSplitOptions.RemoveEmptyEntries)
                                                                   .Select(parameter => parameter.Split('=', 2)[0]));
                    }

                    title = this.WebDriver.Title;
                    bodyText = this.WebDriver.FindElement(By.TagName("body")).Text;
                    bodyText = bodyText.Length > 2000 ? bodyText.Substring(0, 2000) : bodyText;
                }
                catch
                {
                    // Keep diagnostics best-effort so the original test failure is preserved.
                }

                this.TestingContext.DockerHelper.Logger.LogInformation(
                    $"Login screen diagnostic: path=[{path}], queryKeys=[{queryKeys}], title=[{title}], body=[{bodyText}]");
                throw;
            }
        }

        [When(@"I login with the username '([^']*)' and the provided password")]
        public async Task WhenILoginWithTheUsernameAndTheProvidedPassword(string userName)
        {
            this.WebDriver.FillIn("Input.Username", userName);
            this.WebDriver.FillIn("Input.Password", this.TestingContext.Password);
            await this.WebDriver.ClickButton("Login");
        }



        [Then(@"I am presented with the privacy screen")]
        public async Task ThenIAmPresentedWithThePrivacyScreen()
        {
            await Retry.For(async () =>
                            {
                                this.WebDriver.Title.ShouldBe("Privacy Policy - SecurityServiceTestUI");
                            });
            
            
        }

        [When(@"I open the hosted MFA management page")]
        public async Task WhenIOpenTheHostedMfaManagementPage()
        {
            this.WebDriver.Navigate().GoToUrl($"{this.TestingContext.DockerHelper.securityServiceBaseAddressResolver("")}/Account/ManageMfa");
            await Retry.For(async () => this.WebDriver.Title.ShouldContain("Manage MFA"));
        }

        [When(@"I begin MFA enrollment")]
        public async Task WhenIBeginMfaEnrollment()
        {
            await this.WebDriver.ClickButton("Begin enrollment");
        }

        [Then(@"I am shown the MFA authenticator setup key")]
        public void ThenIAmShownTheMfaAuthenticatorSetupKey()
        {
            String body = this.WebDriver.FindElement(By.TagName("body")).Text;
            body.ShouldContain("Setup key:");
            this.TestingContext.MfaAuthenticatorKey = body.Split("Setup key:", StringSplitOptions.None)[1].Split('\n')[0].Trim();
            this.TestingContext.MfaAuthenticatorKey.ShouldNotBeNullOrWhiteSpace();
        }

        [When(@"I confirm MFA enrollment with the current authenticator code")]
        public async Task WhenIConfirmMfaEnrollmentWithTheCurrentAuthenticatorCode()
        {
            this.WebDriver.FillIn("Input.Code", GenerateAuthenticatorCode(this.TestingContext.MfaAuthenticatorKey));
            await this.WebDriver.ClickButton("Confirm enrollment");
        }

        [Then(@"MFA is enabled")]
        public void ThenMfaIsEnabled()
        {
            this.WebDriver.FindElement(By.TagName("body")).Text.ShouldContain("MFA has been enabled.");
        }

        [When(@"I sign out of SecurityService")]
        public async Task WhenISignOutOfSecurityService()
        {
            this.WebDriver.Navigate().GoToUrl($"{this.TestingContext.DockerHelper.securityServiceBaseAddressResolver("")}/Account/Logout");
            await Retry.For(async () => this.WebDriver.Url.ShouldNotContain("/Account/Logout"));
        }

        [Then(@"I am presented with the MFA verification screen")]
        public async Task ThenIAmPresentedWithTheMfaVerificationScreen()
        {
            await Retry.For(async () => this.WebDriver.FindElement(By.TagName("body")).Text.ShouldContain("MFA verification"));
        }

        [When(@"I complete MFA with the current authenticator code")]
        public async Task WhenICompleteMfaWithTheCurrentAuthenticatorCode()
        {
            this.WebDriver.FillIn("Input.Code", GenerateAuthenticatorCode(this.TestingContext.MfaAuthenticatorKey));
            await this.WebDriver.ClickButton("Verify");
        }

        private static String GenerateAuthenticatorCode(String key)
        {
            const String alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            Int32 bits = 0;
            Int32 bitCount = 0;
            List<Byte> keyBytes = new List<Byte>();
            foreach (Char character in key.TrimEnd('=').ToUpperInvariant())
            {
                bits = (bits << 5) | alphabet.IndexOf(character);
                bitCount += 5;
                if (bitCount >= 8)
                {
                    bitCount -= 8;
                    keyBytes.Add((Byte)(bits >> bitCount));
                    bits &= (1 << bitCount) - 1;
                }
            }

            Int64 counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
            Byte[] counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes);
            using HMACSHA1 hmac = new HMACSHA1(keyBytes.ToArray());
            Byte[] hash = hmac.ComputeHash(counterBytes);
            Int32 offset = hash[^1] & 0x0f;
            Int32 binaryCode = ((hash[offset] & 0x7f) << 24) |
                               (hash[offset + 1] << 16) |
                               (hash[offset + 2] << 8) |
                               hash[offset + 3];
            return (binaryCode % 1_000_000).ToString("D6");
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
            var x = StringSerialiser.DeserialiseAnonymousType(await response.Content.ReadAsStringAsync(CancellationToken.None), emailMessage);

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
            var x = StringSerialiser.DeserialiseAnonymousType(await response.Content.ReadAsStringAsync(CancellationToken.None), emailMessage);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(x.Body);
            
            
            var emailNode = doc.DocumentNode.SelectNodes("//*[@id='username']");
            var activationNode = doc.DocumentNode.SelectNodes("//a[@href]");

            emailNode.ShouldHaveSingleItem();
            emailNode.Single().InnerText.ShouldNotBeNullOrEmpty();
            activationNode.ShouldHaveSingleItem();

            this.TestingContext.EmailAddress = emailNode.Single().InnerText.Trim();
            this.TestingContext.Password = "Pa55word!";

            await BrowserNavigation.NavigateWithDiagnosticsAsync(
                () =>
                {
                    this.WebDriver.Navigate().GoToUrl(activationNode.Single().GetAttributeValue("href", string.Empty));
                    return Task.CompletedTask;
                },
                message => this.TestingContext.DockerHelper.Logger.LogInformation(message));

            this.WebDriver.FillIn("Input.Password", this.TestingContext.Password);
            this.WebDriver.FillIn("Input.ConfirmPassword", this.TestingContext.Password);
            await this.WebDriver.ClickButton("Activate account");

            await Retry.For(async () =>
                            {
                                IWebElement activationMessage = this.WebDriver.FindElement(By.Id("activationMessage"));
                                activationMessage.Text.ShouldBe("Your account has been activated. You can now log in.");
                            });
        }


        [When(@"I navigate to the confirm email address")]
        public async Task WhenINavigateToTheConfirmEmailAddress()
        {
            await BrowserNavigation.NavigateWithDiagnosticsAsync(
                () =>
                {
                    this.WebDriver.Navigate().GoToUrl(this.TestingContext.ConfirmEmailAddressLink);
                    return Task.CompletedTask;
                },
                message => this.TestingContext.DockerHelper.Logger.LogInformation(message));
        }

        [Then(@"I am presented with the confirm email address successful screen")]
        public async Task ThenIAmPresentedWithTheConfirmEmailAddressSuccessfulScreen() {
            await Retry.For(async () =>
                            {
                                IWebElement webElement = this.WebDriver.FindElement(By.Id("userMessage"));
                                webElement.Text.ShouldBe("Thanks for confirming your email address, you should receive an account activation email soon.");
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

        public static async Task<IWebElement> FindButton(this IWebDriver webDriver,
                                        String buttonText)
        {
            IWebElement e = null;
            await Retry.For(async () =>
                      {
                          ReadOnlyCollection<IWebElement> elements = webDriver.FindElements(By.TagName("button"));

                          var foundElements = elements.Where(element => element.Text.Trim() == buttonText).ToList();
                          foundElements.ShouldHaveSingleItem();

                          e = foundElements.Single();
                      });

            return e;
        }

        public static async Task ClickLink(this IWebDriver webDriver,
                                        String linkText)
        {
            await Retry.For(async () =>
                            {
                                IWebElement webElement = webDriver.FindElement(By.LinkText(linkText));
                                webElement.ShouldNotBeNull();
                                webElement.Click();
                            });
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
