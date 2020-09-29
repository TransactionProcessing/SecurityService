﻿namespace SecurityService.IntergrationTests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BoDi;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
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
            this.WebDriver.Dispose();
        }

        /// <summary>
        /// Befores the scenario.
        /// </summary>
        [BeforeScenario(Order = 0)]
        public async Task BeforeScenario()
        {
            String? browser = Environment.GetEnvironmentVariable("Browser");
            //browser = "Firefox";

            if (browser == null || browser == "Chrome")
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArguments("--disable-gpu");
                options.AddArguments("--no-sandbox");
                options.AddArguments("--disable-dev-shm-usage");
                var experimentalFlags = new List<String>();
                experimentalFlags.Add("same-site-by-default-cookies@2");
                experimentalFlags.Add("cookies-without-same-site-must-be-secure@2");
                options.AddLocalStatePreference("browser.enabled_labs_experiments", experimentalFlags);
                this.WebDriver = new ChromeDriver(options);
            }

            if (browser == "Firefox")
            {
                FirefoxOptions options = new FirefoxOptions();
                options.AddArguments("-headless");
                this.WebDriver = new FirefoxDriver(options);
            }

            this.ObjectContainer.RegisterInstanceAs(this.WebDriver);
        }

        #endregion
    }
}