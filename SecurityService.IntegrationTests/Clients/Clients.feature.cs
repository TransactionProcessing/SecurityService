﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by Reqnroll (https://www.reqnroll.net/).
//      Reqnroll Version:2.0.0.0
//      Reqnroll Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SecurityService.IntegrationTests.Clients
{
    using Reqnroll;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "2.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Clients")]
    [NUnit.Framework.FixtureLifeCycleAttribute(NUnit.Framework.LifeCycle.InstancePerTestCase)]
    [NUnit.Framework.CategoryAttribute("base")]
    [NUnit.Framework.CategoryAttribute("clients")]
    public partial class ClientsFeature
    {
        
        private global::Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "base",
                "clients"};
        
        private static global::Reqnroll.FeatureInfo featureInfo = new global::Reqnroll.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Clients", "Clients", null, global::Reqnroll.ProgrammingLanguage.CSharp, featureTags);
        
#line 1 "Clients.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public static async System.Threading.Tasks.Task FeatureSetupAsync()
        {
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public static async System.Threading.Tasks.Task FeatureTearDownAsync()
        {
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public async System.Threading.Tasks.Task TestInitializeAsync()
        {
            testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(featureHint: featureInfo);
            if (((testRunner.FeatureContext != null) 
                        && (testRunner.FeatureContext.FeatureInfo.Equals(featureInfo) == false)))
            {
                await testRunner.OnFeatureEndAsync();
            }
            if ((testRunner.FeatureContext == null))
            {
                await testRunner.OnFeatureStartAsync(featureInfo);
            }
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public async System.Threading.Tasks.Task TestTearDownAsync()
        {
            await testRunner.OnScenarioEndAsync();
            global::Reqnroll.TestRunnerManager.ReleaseTestRunner(testRunner);
        }
        
        public void ScenarioInitialize(global::Reqnroll.ScenarioInfo scenarioInfo)
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
        [NUnit.Framework.DescriptionAttribute("Get Clients")]
        [NUnit.Framework.CategoryAttribute("PRTest")]
        public async System.Threading.Tasks.Task GetClients()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Get Clients", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 5
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                global::Reqnroll.Table table9 = new global::Reqnroll.Table(new string[] {
                            "ClientId",
                            "Name",
                            "Description",
                            "Secret",
                            "Scopes",
                            "GrantTypes",
                            "RedirectUris",
                            "PostLogoutRedirectUris",
                            "RequireConsent"});
                table9.AddRow(new string[] {
                            "testclient1",
                            "Test Client 1",
                            "A test client 1",
                            "secret1",
                            "Scope1, Scope2",
                            "client_credentials",
                            "",
                            "",
                            ""});
                table9.AddRow(new string[] {
                            "testclient2",
                            "Test Client 2",
                            "A second test client",
                            "Secret2",
                            "Scope1, Scope2",
                            "hybrid",
                            "http://localhost/signin-oidc",
                            "http://localhost/signout-oidc",
                            "true"});
#line 6
 await testRunner.GivenAsync("I create the following clients", ((string)(null)), table9, "Given ");
#line hidden
                global::Reqnroll.Table table10 = new global::Reqnroll.Table(new string[] {
                            "ClientId",
                            "Name",
                            "Description",
                            "Scopes",
                            "GrantTypes",
                            "RedirectUris",
                            "PostLogoutRedirectUris",
                            "RequireConsent"});
                table10.AddRow(new string[] {
                            "testclient1",
                            "Test Client 1",
                            "A test client 1",
                            "Scope1, Scope2",
                            "client_credentials",
                            "",
                            "",
                            ""});
#line 11
 await testRunner.WhenAsync("I get the client with client id \'testclient1\' the client details are returned as " +
                        "follows", ((string)(null)), table10, "When ");
#line hidden
                global::Reqnroll.Table table11 = new global::Reqnroll.Table(new string[] {
                            "ClientId",
                            "Name",
                            "Description",
                            "Scopes",
                            "GrantTypes",
                            "RedirectUris",
                            "PostLogoutRedirectUris",
                            "RequireConsent"});
                table11.AddRow(new string[] {
                            "testclient2",
                            "Test Client 2",
                            "A second test client",
                            "Scope1, Scope2",
                            "hybrid",
                            "http://localhost/signin-oidc",
                            "http://localhost/signout-oidc",
                            "true"});
#line 15
 await testRunner.WhenAsync("I get the client with client id \'testclient2\' the client details are returned as " +
                        "follows", ((string)(null)), table11, "When ");
#line hidden
                global::Reqnroll.Table table12 = new global::Reqnroll.Table(new string[] {
                            "ClientId",
                            "Name",
                            "Description",
                            "Scopes",
                            "GrantTypes",
                            "RedirectUris",
                            "PostLogoutRedirectUris",
                            "RequireConsent"});
                table12.AddRow(new string[] {
                            "testclient1",
                            "Test Client 1",
                            "A test client 1",
                            "Scope1, Scope2",
                            "client_credentials",
                            "",
                            "",
                            ""});
                table12.AddRow(new string[] {
                            "testclient2",
                            "Test Client 2",
                            "A second test client",
                            "Scope1, Scope2",
                            "hybrid",
                            "http://localhost/signin-oidc",
                            "http://localhost/signout-oidc",
                            "true"});
#line 20
 await testRunner.WhenAsync("I get the clients 2 clients details are returned as follows", ((string)(null)), table12, "When ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
    }
}
#pragma warning restore
#endregion
