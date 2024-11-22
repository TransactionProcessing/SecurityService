namespace SecurityService.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using BusinessLogic.Requests;
    using DataTransferObjects;
    using Microsoft.AspNetCore.Identity;

    public class TestData{
        public static String UserName = "00000001";

        public static String UserName1 = "testaemail1@testing.co.uk";

        public static String UserName2 = "testaemail2@testing.co.uk";

        public static String UserName3 = "testbemail3@testing.co.uk";

        public static String EmailAddress = "testemail1@testing.co.uk";

        public static String EmailAddress1 = "testemail1@testing.co.uk";

        public static String EmailAddress2 = "testemail2@testing.co.uk";

        public static String EmailAddress3 = "testemail3@testing.co.uk";

        public static String Password = "123456";

        public static String NewPassword = "654321";

        public static String PhoneNumber = "07777777777";

        public static List<String> NullRoles = null;

        public static List<String> EmptyRoles = new List<String>();

        public static List<String> Roles => new List<String>{
                                                               TestData.RoleName,
                                                           };

        public static Dictionary<String, String> NullClaims = new Dictionary<String, String>();

        public static Dictionary<String, String> EmptyClaims = new Dictionary<String, String>();

        public static Dictionary<String, String> Claims = new Dictionary<String, String>(){
                                                                                              { "Claim1", "Claim1Value" },
                                                                                              { "Claim2", "Claim2Value" }
                                                                                          };

        public static String UserId = "92BEFFCD-CA30-4E71-B8EC-F38E7DD63A25";

        public static String User1Id = "04911949-321d-4a9b-af31-b259160ba94f";

        public static String User2Id = "b8937737-ec2c-488f-bd95-7d053cb1d36e";

        public static String User3Id = "da6ce793-1ed5-4116-af89-2878b075ec5a";

        public static List<IdentityUser> UserList = new List<IdentityUser>{
                                                                              new IdentityUser{
                                                                                                  UserName = "00000001",
                                                                                                  NormalizedUserName = "00000001",
                                                                                                  Email = "00000001@testemail.com",
                                                                                                  NormalizedEmail = "00000001@testemail.com",
                                                                                                  Id = TestData.User1Id
                                                                                              },
                                                                              new IdentityUser{
                                                                                                  UserName = "00000002",
                                                                                                  NormalizedUserName = "00000002",
                                                                                                  Email = "00000002@testemail.com",
                                                                                                  NormalizedEmail = "00000002@testemail.com",
                                                                                                  Id = TestData.User2Id
                                                                                              },
                                                                              new IdentityUser{
                                                                                                  UserName = "00000003",
                                                                                                  NormalizedUserName = "00000003",
                                                                                                  Email = "00000003@testemail.com",
                                                                                                  NormalizedEmail = "00000003@testemail.com",
                                                                                                  Id = TestData.User3Id
                                                                                              }
                                                                          };

        public static Dictionary<String, List<String>> UserRoles = new Dictionary<String, List<String>>{
                                                                                                           {
                                                                                                               TestData.User1Id, new List<String>(){
                                                                                                                                                       "Role1",
                                                                                                                                                       "Role2"
                                                                                                                                                   }
                                                                                                           },{
                                                                                                                 TestData.User2Id, new List<String>(){
                                                                                                                                                         "Role3"
                                                                                                                                                     }
                                                                                                             },{
                                                                                                                   TestData.User3Id, new List<String>(){
                                                                                                                                                           "Role4"
                                                                                                                                                       }
                                                                                                               },
                                                                                                       };

        public static Dictionary<String, List<Claim>> UserClaims = new Dictionary<String, List<Claim>>{
                                                                                                          {
                                                                                                              TestData.User1Id, new List<Claim>(){
                                                                                                                                                     new Claim("ClaimType1", "Value1")
                                                                                                                                                 }
                                                                                                          },{
                                                                                                                TestData.User2Id, new List<Claim>(){
                                                                                                                                                       new Claim("ClaimType1", "Value2")
                                                                                                                                                   }
                                                                                                            },{
                                                                                                                  TestData.User3Id, new List<Claim>(){
                                                                                                                                                         new Claim("ClaimType1", "Value3")
                                                                                                                                                     }
                                                                                                              },
                                                                                                      };

        public static String GivenName = "GivenName";

        public static String MiddleName = "MiddleName";

        public static String FamilyName = "FamilyName";

        public static IdentityUser IdentityUser = new IdentityUser{
                                                                      Id = TestData.UserId,
                                                                      UserName = TestData.UserName,
                                                                      Email = TestData.EmailAddress,
                                                                      PhoneNumber = TestData.PhoneNumber,
                                                                  };

        public static String ClientId = "testclient";

        public static String ClientSecret = "secretvalue";

        public static String ClientName = "Test Client";

        public static String ClientDescription = "This is a test client";

        public static List<String> AllowedScopes = new List<String>{
                                                                       "Scope1",
                                                                       "Scope2"
                                                                   };

        public static List<String> AllowedGrantTypes = new List<String>{
                                                                           "client_credentials"
                                                                       };
        public static List<String> AllowedGrantTypesWithHybrid = new List<String>{
                                                                           "client_credentials",
                                                                           "hybrid"
                                                                       };

        public static Boolean RequireConsentTrue = true;

        public static Boolean RequireConsentFalse = false;

        public static Boolean AllowOfflineAccessTrue = true;

        public static Boolean AllowOfflineAccessFalse = false;

        public static List<String> EmptyClientRedirectUris = new List<String>();

        public static String ClientUri = "http://localhost";

        public static List<String> ClientRedirectUris = new List<String>{
                                                                            "http://localhost/signin-oidc"
                                                                        };

        public static List<String> EmptyClientPostLogoutRedirectUris = new List<String>();

        public static List<String> ClientPostLogoutRedirectUris = new List<String>{
                                                                                      "http://localhost/signout-oidc"
                                                                                  };

        public static String ApiResourceName = "Test Resource Name";

        public static String IdentityResourceName = "Identity Resource Name";

        public static String IdentityResourceDisplayName = "Identity Display Name";

        public static String ApiResourceDisplayName = "Test Resource Display Name";

        public static String ApiResourceDescription = "Api Resource Description";

        public static String IdentityResourceDescription = "Identity Resource Description";

        public static String ApiResourceSecret = "ApiResourceSecret";

        public static List<String> EmptyApiResourceScopes = new List<String>();

        public static List<String> ApiResourceScopes = new List<String>{
                                                                           "ApiResourceScope1",
                                                                           "ApiResourceScope2"
                                                                       };

        public static List<String> ApiResourceUserClaims = new List<String>{
                                                                               "ApiResourceClaim1",
                                                                               "ApiResourceClaim2"
                                                                           };

        public static List<String> IdentityResourceUserClaims = new List<String>{
                                                                                    "IdentityResourceClaim1",
                                                                                    "IdentityResourceClaim2"
                                                                                };

        public static String RoleName = "TestRole1";

        public static String Role1Id = "12225B6E-36E9-41E0-B786-A1FBFC86C932";

        public async static Task<IQueryable<IdentityUser>> IdentityUsers(){

            IQueryable<IdentityUser> result = new List<IdentityUser>{
                                                                        new IdentityUser{
                                                                                            Id = TestData.User1Id,
                                                                                            UserName = TestData.UserName1,
                                                                                            Email = TestData.EmailAddress1,
                                                                                            PhoneNumber = TestData.PhoneNumber,
                                                                                        },
                                                                        new IdentityUser{
                                                                                            Id = TestData.User2Id,
                                                                                            UserName = TestData.UserName2,
                                                                                            Email = TestData.EmailAddress2,
                                                                                            PhoneNumber = TestData.PhoneNumber,
                                                                                        },
                                                                        new IdentityUser{
                                                                                            Id = TestData.User3Id,
                                                                                            UserName = TestData.UserName3,
                                                                                            Email = TestData.EmailAddress3,
                                                                                            PhoneNumber = TestData.PhoneNumber,
                                                                                        }
                                                                    }.AsQueryable();

            return await Task.FromResult(result);
        }

        public static IdentityRole IdentityRole = new IdentityRole{
                                                                      Id = TestData.Role1Id,
                                                                      Name = TestData.RoleName
                                                                  };

        public static String ApiScopeName = "Test Api Scope";

        public static String ApiScopeDisplayName = "Test Api Scope Display Name";

        public static String ApiScopeDescription = "Test Api Scope Description";

        public static SecurityServiceCommands.CreateUserCommand CreateUserCommand =>
            new(Guid.Parse(TestData.UserId),
                                     TestData.GivenName,
                                     TestData.MiddleName,
                                     TestData.FamilyName,
                                     TestData.UserName,
                                     TestData.Password,
                                     TestData.EmailAddress,
                                     TestData.PhoneNumber,
                                     TestData.Claims,
                                     TestData.Roles);

        public static SecurityServiceQueries.GetUserQuery GetUserQuery => new(Guid.Parse(TestData.UserId));
        public static SecurityServiceQueries.GetUsersQuery GetUsersQuery => new(null);

        public static SecurityServiceCommands.CreateApiResourceCommand CreateApiResourceCommand =>
            new(TestData.ApiResourceName,
                                            TestData.ApiResourceDisplayName,
                                            TestData.ApiResourceDescription,
                                            TestData.ApiResourceSecret,
                                            TestData.ApiResourceScopes,
                                            TestData.ApiResourceUserClaims);

        public static SecurityServiceQueries.GetApiResourceQuery GetApiResourceQuery => new(TestData.ApiResourceName);
        public static SecurityServiceQueries.GetApiResourcesQuery GetApiResourcesQuery => new();

        public static SecurityServiceCommands.CreateClientCommand CreateClientCommand => new(TestData.ClientId,
                                                                                             TestData.ClientSecret,
                                                                                             TestData.ClientName,
                                                                                             TestData.ClientDescription,
                                                                                             TestData.AllowedScopes,
                                                                                             TestData.AllowedGrantTypes,
                                                                                             TestData.ClientUri,
                                                                                             TestData.ClientRedirectUris,
                                                                                             TestData.ClientPostLogoutRedirectUris,
                                                                                             TestData.RequireConsentTrue,
                                                                                             TestData.AllowOfflineAccessTrue);

        public static SecurityServiceCommands.CreateClientCommand CreateHybridClientCommand =>
            new(TestData.ClientId,
                                       TestData.ClientSecret,
                                       TestData.ClientName,
                                       TestData.ClientDescription,
                                       TestData.AllowedScopes,
                                       TestData.AllowedGrantTypesWithHybrid,
                                       TestData.ClientUri,
                                       TestData.ClientRedirectUris,
                                       TestData.ClientPostLogoutRedirectUris,
                                       TestData.RequireConsentTrue,
                                       TestData.AllowOfflineAccessTrue);

        public static SecurityServiceQueries.GetClientQuery GetClientQuery => new(TestData.ClientId);

        public static SecurityServiceQueries.GetClientsQuery GetClientsQuery => new();

        public static SecurityServiceCommands.CreateApiScopeCommand CreateApiScopeCommand  => new(TestData.ApiScopeName,
                                                                                                  TestData.ApiScopeDisplayName,
                                                                                                  TestData.ApiScopeDescription);

        public static SecurityServiceCommands.CreateIdentityResourceCommand CreateIdentityResourceCommand => new(TestData.IdentityResourceName,
                                                                                                                          TestData.IdentityResourceDisplayName,
                                                                                                                          TestData.IdentityResourceDescription,
                                                                                                                          TestData.IdentityResourceRequired,
                                                                                                                          TestData.IdentityResourceEmphasize,
                                                                                                                          TestData.IdentityResourceShowInDiscoveryDocument,
                                                                                                                          TestData.IdentityResourceUserClaims);
        
        public static SecurityServiceCommands.CreateRoleCommand CreateRoleCommand => new(Guid.Parse(TestData.Role1Id), TestData.RoleName);

        public static SecurityServiceQueries.GetApiScopeQuery GetApiScopeQuery => new(TestData.ApiScopeName);
        public static SecurityServiceQueries.GetApiScopesQuery GetApiScopesQuery => new();

        public static SecurityServiceQueries.GetIdentityResourceQuery GetIdentityResourceQuery => new(TestData.IdentityResourceName);

        public static SecurityServiceQueries.GetIdentityResourcesQuery GetIdentityResourcesQuery => new();

        public static SecurityServiceQueries.GetRoleQuery GetRoleQuery => new(Guid.Parse(TestData.Role1Id));
        public static SecurityServiceQueries.GetRolesQuery GetRolesQuery => new();

        public static Boolean IdentityResourceRequired = true;

        public static Boolean IdentityResourceEmphasize = true;

        public static Boolean IdentityResourceShowInDiscoveryDocument = true;

        public static String ConfirmEmailToken = "Token";

        public static String PasswordResetToken = "Token";

        public static SecurityServiceCommands.ChangeUserPasswordCommand ChangeUserPasswordCommand =>
            new(TestData.UserName,
                                             TestData.Password,
                                             TestData.Password,
                                             TestData.ClientId);
        public static SecurityServiceCommands.ConfirmUserEmailAddressCommand ConfirmUserEmailAddressCommand => new(TestData.UserName,
                                                  TestData.ConfirmEmailToken);
        public static SecurityServiceCommands.ProcessPasswordResetConfirmationCommand ProcessPasswordResetConfirmationCommand =>new (TestData.UserName,
                                                           TestData.PasswordResetToken,
                                                           TestData.Password,
                                                           TestData.ClientId);

        public static SecurityServiceCommands.ProcessPasswordResetRequestCommand ProcessPasswordResetRequestCommand => new(TestData.UserName, TestData.EmailAddress, TestData.ClientId);

        public static SecurityServiceCommands.SendWelcomeEmailCommand SendWelcomeEmailCommand  => new(TestData.UserName);
    }
}

    
