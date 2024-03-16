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
namespace SecurityService.OpenIdConnect.IntegrationTests.ChangePassword
{
    using Reqnroll;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "1.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "base")]
    [Xunit.TraitAttribute("Category", "shared")]
    [Xunit.TraitAttribute("Category", "userlogin")]
    [Xunit.TraitAttribute("Category", "changepassword")]
    public partial class ChangePasswordFeature : object, Xunit.IClassFixture<ChangePasswordFeature.FixtureData>, Xunit.IAsyncLifetime
    {
        
        private static Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "base",
                "shared",
                "userlogin",
                "changepassword"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "ChangePassword.feature"
#line hidden
        
        public ChangePasswordFeature(ChangePasswordFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }
        
        public static async System.Threading.Tasks.Task FeatureSetupAsync()
        {
            testRunner = Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.GetWorkerId());
            Reqnroll.FeatureInfo featureInfo = new Reqnroll.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ChangePassword", "Change Password", null, ProgrammingLanguage.CSharp, featureTags);
            await testRunner.OnFeatureStartAsync(featureInfo);
        }
        
        public static async System.Threading.Tasks.Task FeatureTearDownAsync()
        {
            string testWorkerId = testRunner.TestWorkerId;
            await testRunner.OnFeatureEndAsync();
            testRunner = null;
            Reqnroll.xUnit.ReqnrollPlugin.XUnitParallelWorkerTracker.Instance.ReleaseWorker(testWorkerId);
        }
        
        public async System.Threading.Tasks.Task TestInitializeAsync()
        {
        }
        
        public async System.Threading.Tasks.Task TestTearDownAsync()
        {
            await testRunner.OnScenarioEndAsync();
        }
        
        public void ScenarioInitialize(Reqnroll.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public async System.Threading.Tasks.Task ScenarioStartAsync()
        {
            await testRunner.OnScenarioStartAsync();
        }
        
        public async System.Threading.Tasks.Task ScenarioCleanupAsync()
        {
            await testRunner.CollectScenarioErrorsAsync();
        }
        
        public virtual async System.Threading.Tasks.Task FeatureBackgroundAsync()
        {
#line 4
#line hidden
            Reqnroll.Table table1 = new Reqnroll.Table(new string[] {
                        "Role Name"});
            table1.AddRow(new string[] {
                        "Estate"});
#line 6
 await testRunner.GivenAsync("I create the following roles", ((string)(null)), table1, "Given ");
#line hidden
            Reqnroll.Table table2 = new Reqnroll.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Secret",
                        "Scopes",
                        "UserClaims"});
            table2.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST",
                        "Secret1",
                        "estateManagement",
                        "MerchantId,EstateId,role"});
#line 10
 await testRunner.GivenAsync("I create the following api resources", ((string)(null)), table2, "Given ");
#line hidden
            Reqnroll.Table table3 = new Reqnroll.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Description",
                        "UserClaims"});
            table3.AddRow(new string[] {
                        "openid",
                        "Your user identifier",
                        "",
                        "sub"});
            table3.AddRow(new string[] {
                        "profile",
                        "User profile",
                        "Your user profile information (first name, last name, etc.)",
                        "name,role,email,given_name,middle_name,family_name,EstateId,MerchantId"});
            table3.AddRow(new string[] {
                        "email",
                        "Email",
                        "Email and Email Verified Flags",
                        "email_verified,email"});
#line 14
 await testRunner.GivenAsync("I create the following identity resources", ((string)(null)), table3, "Given ");
#line hidden
            Reqnroll.Table table4 = new Reqnroll.Table(new string[] {
                        "ClientId",
                        "Name",
                        "Secret",
                        "Scopes",
                        "GrantTypes",
                        "RedirectUris",
                        "PostLogoutRedirectUris",
                        "RequireConsent",
                        "AllowOfflineAccess",
                        "ClientUri"});
            table4.AddRow(new string[] {
                        "estateUIClient",
                        "Merchant Client",
                        "Secret1",
                        "estateManagement,openid,email,profile",
                        "hybrid",
                        "https://[url]:[port]/signin-oidc",
                        "https://[url]:[port]/signout-oidc",
                        "false",
                        "true",
                        "https://[url]:[port]"});
#line 20
 await testRunner.GivenAsync("I create the following clients", ((string)(null)), table4, "Given ");
#line hidden
        }
        
        async System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
        {
            await this.TestInitializeAsync();
        }
        
        async System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
        {
            await this.TestTearDownAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Change Passwword")]
        [Xunit.TraitAttribute("FeatureTitle", "Change Password")]
        [Xunit.TraitAttribute("Description", "Change Passwword")]
        [Xunit.TraitAttribute("Category", "PRTest")]
        public async System.Threading.Tasks.Task ChangePasswword()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Change Passwword", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 26
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 4
await this.FeatureBackgroundAsync();
#line hidden
                Reqnroll.Table table5 = new Reqnroll.Table(new string[] {
                            "Email Address",
                            "Phone Number",
                            "Given Name",
                            "Middle Name",
                            "Family Name",
                            "Claims",
                            "Roles"});
                table5.AddRow(new string[] {
                            "estateuser@testestate1.co.uk",
                            "123456789",
                            "Test",
                            "",
                            "User 1",
                            "EstateId:1",
                            "Estate"});
#line 27
 await testRunner.GivenAsync("I create the following users", ((string)(null)), table5, "Given ");
#line hidden
#line 30
 await testRunner.ThenAsync("I get an email with a confirm email address link", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
#line 31
 await testRunner.WhenAsync("I navigate to the confirm email address", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 32
 await testRunner.ThenAsync("I am presented with the confirm email address successful screen", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
#line 33
 await testRunner.AndAsync("I get a welcome email with my login details", ((string)(null)), ((Reqnroll.Table)(null)), "And ");
#line hidden
#line 34
 await testRunner.GivenAsync("I am on the application home page", ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 35
 await testRunner.WhenAsync("I click the \'Privacy\' link", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 36
 await testRunner.ThenAsync("I am presented with a login screen", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
#line 37
 await testRunner.WhenAsync("I login with the username \'estateuser@testestate1.co.uk\' and the provided passwor" +
                        "d", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 38
 await testRunner.ThenAsync("I am presented with the privacy screen", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
#line 39
 await testRunner.WhenAsync("I click the \'ChangePassword\' link", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 40
 await testRunner.ThenAsync("I am presented with a change password screen", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
#line 41
 await testRunner.WhenAsync("I enter my old password \'123456\'", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 42
 await testRunner.WhenAsync("I enter my new password \'Pa55word!\'", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 43
 await testRunner.AndAsync("I confirm my new password \'Pa55word!\'", ((string)(null)), ((Reqnroll.Table)(null)), "And ");
#line hidden
#line 44
 await testRunner.AndAsync("I click the change password button", ((string)(null)), ((Reqnroll.Table)(null)), "And ");
#line hidden
#line 45
 await testRunner.ThenAsync("I am returned to the application home page", ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "1.0.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : object, Xunit.IAsyncLifetime
        {
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
            {
                await ChangePasswordFeature.FeatureSetupAsync();
            }
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
            {
                await ChangePasswordFeature.FeatureTearDownAsync();
            }
        }
    }
}
#pragma warning restore
#endregion
