using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.IntergrationTests.Common
{
    using System.Threading.Tasks;
    using TechTalk.SpecFlow;

    [Binding]
    [Scope(Tag = "base")]
    public class GenericSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        public GenericSteps(ScenarioContext scenarioContext,
                            TestingContext testingContext)
        {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
        }

        [BeforeScenario(Order=1)]
        public async Task StartSystem()
        {
            String scenarioName = this.ScenarioContext.ScenarioInfo.Title.Replace(" ", "");
            this.TestingContext.DockerHelper = new DockerHelper();
            await this.TestingContext.DockerHelper.StartContainersForScenarioRun(scenarioName).ConfigureAwait(false);
        }

        [AfterScenario(Order=1)]
        public async Task StopSystem()
        {
            await this.TestingContext.DockerHelper.StopContainersForScenarioRun().ConfigureAwait(false);
        }
    }
}
