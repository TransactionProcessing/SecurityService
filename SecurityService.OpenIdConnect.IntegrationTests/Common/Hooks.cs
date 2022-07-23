namespace SecurityService.IntergrationTests.Common
{
    using System;
    using System.Threading.Tasks;
    using BoDi;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Edge;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.Remote;
    using TechTalk.SpecFlow;

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
            browser = "Edge";

            if (browser == null || browser == "Chrome")
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArguments("--disable-gpu");
                options.AddArguments("--no-sandbox");
                options.AddArguments("--disable-dev-shm-usage");
                options.AcceptInsecureCertificates = true;

                ChromeDriverService x = ChromeDriverService.CreateDefaultService();

                this.WebDriver = new ChromeDriver(x, options, TimeSpan.FromMinutes(3));
            }

            if (browser == "Firefox")
            {
                FirefoxOptions options = new FirefoxOptions();
                options.AcceptInsecureCertificates = true;
                options.AddArguments("-headless");
                options.LogLevel = FirefoxDriverLogLevel.Debug;
                FirefoxDriverService x = FirefoxDriverService.CreateDefaultService();
                
                this.WebDriver = new FirefoxDriver(x, options, TimeSpan.FromMinutes(3));
            }

            if (browser == "Edge")
            {
                EdgeOptions options = new EdgeOptions();
                options.AcceptInsecureCertificates = true;
                EdgeDriverService x = EdgeDriverService.CreateDefaultService();

                this.WebDriver = new EdgeDriver(x,options, TimeSpan.FromMinutes(3));
            }

            this.WebDriver.Manage().Timeouts().PageLoad.Add(TimeSpan.FromSeconds(30));

            this.ObjectContainer.RegisterInstanceAs(this.WebDriver);
        }

        #endregion
    }
}
