﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by Reqnroll (https://www.reqnroll.net/).
//      Reqnroll Version:1.0.0.0
//      Reqnroll Generator Version:1.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SecurityService.IntegrationTests.ApiScopes
{
    using Reqnroll;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "1.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("ApiScope")]
    [NUnit.Framework.CategoryAttribute("base")]
    [NUnit.Framework.CategoryAttribute("apiscopes")]
    public partial class ApiScopeFeature
    {
        
        private Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "base",
                "apiscopes"};
        
#line 1 "ApiScope.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual async System.Threading.Tasks.Task FeatureSetupAsync()
        {
            testRunner = Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, NUnit.Framework.TestContext.CurrentContext.WorkerId);
            Reqnroll.FeatureInfo featureInfo = new Reqnroll.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ApiScopes", "ApiScope", null, ProgrammingLanguage.CSharp, featureTags);
            await testRunner.OnFeatureStartAsync(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual async System.Threading.Tasks.Task FeatureTearDownAsync()
        {
            await testRunner.OnFeatureEndAsync();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public async System.Threading.Tasks.Task TestInitializeAsync()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public async System.Threading.Tasks.Task TestTearDownAsync()
        {
            await testRunner.OnScenarioEndAsync();
        }
        
        public void ScenarioInitialize(Reqnroll.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public async System.Threading.Tasks.Task ScenarioStartAsync()
        {
            await testRunner.OnScenarioStartAsync();
        }
        
        public async System.Threading.Tasks.Task ScenarioCleanupAsync()
        {
            await testRunner.CollectScenarioErrorsAsync();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Get Api Scopes")]
        [NUnit.Framework.CategoryAttribute("PRTest")]
        public async System.Threading.Tasks.Task GetApiScopes()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Get Api Scopes", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 5
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                Reqnroll.Table table5 = new Reqnroll.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description"});
                table5.AddRow(new string[] {
                            "testscope1",
                            "Test Scope1",
                            "A scope for testing 1"});
                table5.AddRow(new string[] {
                            "testscope2",
                            "Test Scope2",
                            "A scope for testing 2"});
#line 6
 await testRunner.GivenAsync("I create the following api scopes", ((string)(null)), table5, "Given ");
#line hidden
                Reqnroll.Table table6 = new Reqnroll.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description"});
                table6.AddRow(new string[] {
                            "testscope1",
                            "Test Scope1",
                            "A scope for testing 1"});
#line 10
 await testRunner.WhenAsync("I get the api scope with name \'testscope1\' the api scope details are returned as " +
                        "follows", ((string)(null)), table6, "When ");
#line hidden
                Reqnroll.Table table7 = new Reqnroll.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description"});
                table7.AddRow(new string[] {
                            "testscope2",
                            "Test Scope2",
                            "A scope for testing 2"});
#line 13
 await testRunner.WhenAsync("I get the api scope with name \'testscope2\' the api scope details are returned as " +
                        "follows", ((string)(null)), table7, "When ");
#line hidden
                Reqnroll.Table table8 = new Reqnroll.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description"});
                table8.AddRow(new string[] {
                            "testscope1",
                            "Test Scope1",
                            "A scope for testing 1"});
                table8.AddRow(new string[] {
                            "testscope2",
                            "Test Scope2",
                            "A scope for testing 2"});
#line 16
 await testRunner.WhenAsync("I get the api scopes 2 api scope details are returned as follows", ((string)(null)), table8, "When ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
    }
}
#pragma warning restore
#endregion
