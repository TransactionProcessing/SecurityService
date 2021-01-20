﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.5.0.0
//      SpecFlow Generator Version:3.5.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SecurityService.IntegrationTests.Users
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.5.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "base")]
    [Xunit.TraitAttribute("Category", "users")]
    [Xunit.TraitAttribute("Category", "roles")]
    public partial class UsersFeature : object, Xunit.IClassFixture<UsersFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private string[] _featureTags = new string[] {
                "base",
                "users",
                "roles"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "Users.feature"
#line hidden
        
        public UsersFeature(UsersFeature.FixtureData fixtureData, SecurityService_IntegrationTests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Users", "Users", null, ProgrammingLanguage.CSharp, new string[] {
                        "base",
                        "users",
                        "roles"});
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void TestInitialize()
        {
        }
        
        public virtual void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 4
#line hidden
            TechTalk.SpecFlow.Table table34 = new TechTalk.SpecFlow.Table(new string[] {
                        "Role Name"});
            table34.AddRow(new string[] {
                        "TestRole1"});
            table34.AddRow(new string[] {
                        "TestRole2"});
            table34.AddRow(new string[] {
                        "TestRole3"});
#line 5
 testRunner.Given("I create the following roles", ((string)(null)), table34, "Given ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Create User")]
        [Xunit.TraitAttribute("FeatureTitle", "Users")]
        [Xunit.TraitAttribute("Description", "Create User")]
        public virtual void CreateUser()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create User", null, tagsOfScenario, argumentsOfScenario);
#line 11
this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table35 = new TechTalk.SpecFlow.Table(new string[] {
                            "Email Address",
                            "Password",
                            "Phone Number",
                            "Given Name",
                            "Middle Name",
                            "Family Name",
                            "Claims",
                            "Roles"});
                table35.AddRow(new string[] {
                            "testuser1@testing.co.uk",
                            "123456",
                            "123456789",
                            "Test",
                            "",
                            "User 1",
                            "Claim1:value1,Claim2:value2",
                            "TestRole1"});
#line 12
 testRunner.Given("I create the following users", ((string)(null)), table35, "Given ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get User")]
        [Xunit.TraitAttribute("FeatureTitle", "Users")]
        [Xunit.TraitAttribute("Description", "Get User")]
        public virtual void GetUser()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get User", null, tagsOfScenario, argumentsOfScenario);
#line 16
this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table36 = new TechTalk.SpecFlow.Table(new string[] {
                            "Email Address",
                            "Password",
                            "Phone Number",
                            "Given Name",
                            "Middle Name",
                            "Family Name",
                            "Claims",
                            "Roles"});
                table36.AddRow(new string[] {
                            "testuser1@testing.co.uk",
                            "123456",
                            "123456789",
                            "Test",
                            "",
                            "User 1",
                            "",
                            "TestRole1"});
                table36.AddRow(new string[] {
                            "testuser2@testing.co.uk",
                            "123456",
                            "123456789",
                            "Test",
                            "",
                            "User 2",
                            "",
                            "TestRole2"});
#line 17
 testRunner.Given("I create the following users", ((string)(null)), table36, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table37 = new TechTalk.SpecFlow.Table(new string[] {
                            "Email Address",
                            "Phone Number",
                            "Given Name",
                            "Middle Name",
                            "Family Name",
                            "Claims",
                            "Roles"});
                table37.AddRow(new string[] {
                            "testuser1@testing.co.uk",
                            "123456789",
                            "Test",
                            "",
                            "User 1",
                            "email:testuser1@testing.co.uk, given_name:Test, family_name:User 1",
                            "TestRole1"});
#line 21
 testRunner.When("I get the user with user name \'testuser1@testing.co.uk\' the user details are retu" +
                        "rned as follows", ((string)(null)), table37, "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get Users")]
        [Xunit.TraitAttribute("FeatureTitle", "Users")]
        [Xunit.TraitAttribute("Description", "Get Users")]
        [Xunit.TraitAttribute("Category", "PRTest")]
        public virtual void GetUsers()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get Users", null, tagsOfScenario, argumentsOfScenario);
#line 26
this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table38 = new TechTalk.SpecFlow.Table(new string[] {
                            "Email Address",
                            "Password",
                            "Phone Number",
                            "Given Name",
                            "Middle Name",
                            "Family Name",
                            "Claims",
                            "Roles"});
                table38.AddRow(new string[] {
                            "testuser1@testing.co.uk",
                            "123456",
                            "123456789",
                            "Test",
                            "",
                            "User 1",
                            "",
                            "TestRole1"});
                table38.AddRow(new string[] {
                            "testuser2@testing.co.uk",
                            "123456",
                            "123456789",
                            "Test",
                            "",
                            "User 2",
                            "",
                            "TestRole2"});
                table38.AddRow(new string[] {
                            "testuser3@testing.co.uk",
                            "123456",
                            "123456789",
                            "Test",
                            "",
                            "User 3",
                            "",
                            "TestRole3"});
#line 27
 testRunner.Given("I create the following users", ((string)(null)), table38, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table39 = new TechTalk.SpecFlow.Table(new string[] {
                            "Email Address",
                            "Phone Number",
                            "Given Name",
                            "Middle Name",
                            "Family Name",
                            "Claims",
                            "Roles"});
                table39.AddRow(new string[] {
                            "testuser1@testing.co.uk",
                            "123456789",
                            "Test",
                            "",
                            "User 1",
                            "email:testuser1@testing.co.uk, given_name:Test, family_name:User 1",
                            "TestRole1"});
                table39.AddRow(new string[] {
                            "testuser2@testing.co.uk",
                            "123456789",
                            "Test",
                            "",
                            "User 2",
                            "email:testuser2@testing.co.uk, given_name:Test, family_name:User 2",
                            "TestRole2"});
                table39.AddRow(new string[] {
                            "testuser3@testing.co.uk",
                            "123456789",
                            "Test",
                            "",
                            "User 3",
                            "email:testuser3@testing.co.uk, given_name:Test, family_name:User 3",
                            "TestRole3"});
#line 32
 testRunner.When("I get the users 3 users details are returned as follows", ((string)(null)), table39, "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.5.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                UsersFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                UsersFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
