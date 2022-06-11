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
namespace SecurityService.IntegrationTests.ApiResource
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "base")]
    [Xunit.TraitAttribute("Category", "apiresources")]
    public partial class ApiResourceFeature : object, Xunit.IClassFixture<ApiResourceFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "base",
                "apiresources"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "ApiResource.feature"
#line hidden
        
        public ApiResourceFeature(ApiResourceFeature.FixtureData fixtureData, SecurityService_IntegrationTests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ApiResource", "ApiResource", null, ProgrammingLanguage.CSharp, featureTags);
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
        
        [Xunit.SkippableFactAttribute(DisplayName="Create Api Resource")]
        [Xunit.TraitAttribute("FeatureTitle", "ApiResource")]
        [Xunit.TraitAttribute("Description", "Create Api Resource")]
        public void CreateApiResource()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create Api Resource", null, tagsOfScenario, argumentsOfScenario, featureTags);
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
                TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description",
                            "Secret",
                            "Scopes",
                            "UserClaims"});
                table1.AddRow(new string[] {
                            "testresource",
                            "Test Resource",
                            "A resource for testing",
                            "secret1",
                            "Scope1,Scope2",
                            "Claim1,Claim2"});
#line 5
 testRunner.Given("I create the following api resources", ((string)(null)), table1, "Given ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get Api Resource")]
        [Xunit.TraitAttribute("FeatureTitle", "ApiResource")]
        [Xunit.TraitAttribute("Description", "Get Api Resource")]
        public void GetApiResource()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get Api Resource", null, tagsOfScenario, argumentsOfScenario, featureTags);
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
                TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description",
                            "Secret",
                            "Scopes",
                            "UserClaims"});
                table2.AddRow(new string[] {
                            "testresource1",
                            "Test Resource1",
                            "A resource for testing 1",
                            "secret1",
                            "Scope1,Scope2",
                            "Claim1,Claim2"});
                table2.AddRow(new string[] {
                            "testresource2",
                            "Test Resource2",
                            "A resource for testing 2",
                            "secret2",
                            "Scope1,Scope2",
                            "Claim1,Claim2"});
#line 10
 testRunner.Given("I create the following api resources", ((string)(null)), table2, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description",
                            "Scopes",
                            "UserClaims"});
                table3.AddRow(new string[] {
                            "testresource1",
                            "Test Resource1",
                            "A resource for testing 1",
                            "Scope1,Scope2",
                            "Claim1,Claim2"});
#line 14
 testRunner.When("I get the api resource with name \'testresource1\' the api resource details are ret" +
                        "urned as follows", ((string)(null)), table3, "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get Api Resources")]
        [Xunit.TraitAttribute("FeatureTitle", "ApiResource")]
        [Xunit.TraitAttribute("Description", "Get Api Resources")]
        [Xunit.TraitAttribute("Category", "PRTest")]
        public void GetApiResources()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get Api Resources", null, tagsOfScenario, argumentsOfScenario, featureTags);
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
                TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description",
                            "Secret",
                            "Scopes",
                            "UserClaims"});
                table4.AddRow(new string[] {
                            "testresource1",
                            "Test Resource1",
                            "A resource for testing 1",
                            "secret1",
                            "Scope1,Scope2",
                            "Claim1,Claim2"});
                table4.AddRow(new string[] {
                            "testresource2",
                            "Test Resource2",
                            "A resource for testing 2",
                            "secret2",
                            "Scope1,Scope2",
                            "Claim1,Claim2"});
#line 20
testRunner.Given("I create the following api resources", ((string)(null)), table4, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "DisplayName",
                            "Description",
                            "Scopes",
                            "UserClaims"});
                table5.AddRow(new string[] {
                            "testresource1",
                            "Test Resource1",
                            "A resource for testing 1",
                            "Scope1,Scope2",
                            "Claim1,Claim2"});
                table5.AddRow(new string[] {
                            "testresource2",
                            "Test Resource2",
                            "A resource for testing 2",
                            "Scope1,Scope2",
                            "Claim1,Claim2"});
#line 24
 testRunner.When("I get the api resources 2 api resource details are returned as follows", ((string)(null)), table5, "When ");
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
                ApiResourceFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                ApiResourceFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
