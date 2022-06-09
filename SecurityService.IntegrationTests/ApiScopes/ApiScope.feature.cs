﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SecurityService.IntegrationTests.ApiScopes
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "base")]
    [Xunit.TraitAttribute("Category", "apiscopes")]
    public partial class ApiScopeFeature : object, Xunit.IClassFixture<ApiScopeFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "base",
                "apiscopes"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "ApiScope.feature"
#line hidden
        
        public ApiScopeFeature(ApiScopeFeature.FixtureData fixtureData, SecurityService_IntegrationTests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ApiScopes", "ApiScope", null, ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public void TestInitialize()
        {
        }
        
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Create Api Scope")]
        [Xunit.TraitAttribute("FeatureTitle", "ApiScope")]
        [Xunit.TraitAttribute("Description", "Create Api Scope")]
        public void CreateApiScope()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create Api Scope", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 4
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description"});
                table6.AddRow(new string[] {
                            "testscope",
                            "Test Scope",
                            "A scope for testing"});
#line 5
 testRunner.Given("I create the following api scopes", ((string)(null)), table6, "Given ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get Api Scope")]
        [Xunit.TraitAttribute("FeatureTitle", "ApiScope")]
        [Xunit.TraitAttribute("Description", "Get Api Scope")]
        public void GetApiScope()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get Api Scope", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 9
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description"});
                table7.AddRow(new string[] {
                            "testscope1",
                            "Test Scope1",
                            "A scope for testing 1"});
                table7.AddRow(new string[] {
                            "testscope2",
                            "Test Scope2",
                            "A scope for testing 2"});
#line 10
 testRunner.Given("I create the following api scopes", ((string)(null)), table7, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description"});
                table8.AddRow(new string[] {
                            "testscope1",
                            "Test Scope1",
                            "A scope for testing 1"});
#line 14
 testRunner.When("I get the api scope with name \'testscope1\' the api scope details are returned as " +
                        "follows", ((string)(null)), table8, "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get Api Scopes")]
        [Xunit.TraitAttribute("FeatureTitle", "ApiScope")]
        [Xunit.TraitAttribute("Description", "Get Api Scopes")]
        [Xunit.TraitAttribute("Category", "PRTest")]
        public void GetApiScopes()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get Api Scopes", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 19
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description"});
                table9.AddRow(new string[] {
                            "testscope1",
                            "Test Scope1",
                            "A scope for testing 1"});
                table9.AddRow(new string[] {
                            "testscope2",
                            "Test Scope2",
                            "A scope for testing 2"});
#line 20
testRunner.Given("I create the following api scopes", ((string)(null)), table9, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description"});
                table10.AddRow(new string[] {
                            "testscope1",
                            "Test Scope1",
                            "A scope for testing 1"});
                table10.AddRow(new string[] {
                            "testscope2",
                            "Test Scope2",
                            "A scope for testing 2"});
#line 24
 testRunner.When("I get the api scopes 2 api scope details are returned as follows", ((string)(null)), table10, "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                ApiScopeFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                ApiScopeFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
