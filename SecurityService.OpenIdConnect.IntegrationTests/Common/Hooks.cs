using Reqnroll.BoDi;

namespace SecurityService.IntergrationTests.Common
{
    using System;
    using System.Threading.Tasks;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Edge;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.Remote;
    using Reqnroll;
    using Shared.IntegrationTesting;

    /// <summary>
    /// 
    /// </summary>
    [Binding]
    public class Hooks
    {
        #region Fields

        /// <summary>
        /// The object container
        /// </summary>
        private readonly IObjectContainer ObjectContainer;

        /// <summary>
        /// The web driver
        /// </summary>
        private IWebDriver WebDriver;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Hooks"/> class.
        /// </summary>
        /// <param name="objectContainer">The object container.</param>
        public Hooks(IObjectContainer objectContainer)
        {
            this.ObjectContainer = objectContainer;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Afters the scenario.
        /// </summary>
        [AfterScenario(Order = 0)]
        public void AfterScenario()
        {
            if (this.WebDriver != null)
            {
                this.WebDriver.Dispose();
            }
        }

        /// <summary>
        /// Befores the scenario.
        /// </summary>
        [BeforeScenario(Order = 0)]
        public async Task BeforeScenario()
        {
            String? browser = Environment.GetEnvironmentVariable("Browser");
            //browser = "Edge";
            if (browser == null || browser == "Chrome")
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArguments("--disable-gpu");
                options.AddArguments("--headless");
                options.AddArguments("--no-sandbox");
                options.AddArguments("--disable-dev-shm-usage");
                options.AddArguments("disable-infobars");
                options.AddArguments("--disable-extensions");
                options.AddArguments("--window-size=1280x1024");
                options.AcceptInsecureCertificates = true;
                this.WebDriver = new ChromeDriver(options);
                this.WebDriver.Manage().Window.Maximize();
            }

            if (browser == "Firefox")
            {
                FirefoxOptions options = new FirefoxOptions();
                options.AddArguments("--headless");
                options.SetPreference("network.cookie.cookieBehavior", 0);
                options.AcceptInsecureCertificates = true;

                await Retry.For(async () =>
                                {
                                    this.WebDriver = new FirefoxDriver(options);
                                }, TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(60));

                this.WebDriver.Manage().Window.Maximize();
            }

            if (browser == "Edge")
            {
                EdgeOptions options = new EdgeOptions();
                options.AcceptInsecureCertificates = true;
                options.AddArguments("--headless");
                options.AddArguments("--window-size=1280x1024");
                await Retry.For(async () =>
                                {
                                    this.WebDriver = new EdgeDriver(options);
                                }, TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(60));
                this.WebDriver.Manage().Window.Maximize();
            }

            this.ObjectContainer.RegisterInstanceAs(this.WebDriver);
        }

        #endregion
    }
}
