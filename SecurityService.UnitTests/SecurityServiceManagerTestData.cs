namespace SecurityService.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using Microsoft.AspNetCore.Identity;

    public class SecurityServiceManagerTestData
    {
        public static String UserName = "00000001";
        public static String UserName1 = "testaemail1@testing.co.uk";
        public static String UserName2 = "testaemail2@testing.co.uk";
        public static String UserName3 = "testbemail3@testing.co.uk";
        public static String EmailAddress = "testemail1@testing.co.uk";
        public static String EmailAddress1 = "testemail1@testing.co.uk";
        public static String EmailAddress2 = "testemail2@testing.co.uk";
        public static String EmailAddress3 = "testemail3@testing.co.uk";
        public static String Password = "123456";
        public static String PhoneNumber = "07777777777";

        public static List<String> Roles = new List<String>
        {
            //"Role1",
            //"Role2"
        };

        public static Dictionary<String, String> Claims = new Dictionary<String, String>()
        {
            //{"Claim1", "Claim1Value"},
            //{"Claim2", "Claim2Value"}
        };

        public static CreateUserRequest GetCreateUserRequest
        {
            get
            {
                CreateUserRequest request = new CreateUserRequest();

                request.GivenName = SecurityServiceManagerTestData.GivenName;
                request.MiddleName = SecurityServiceManagerTestData.MiddleName;
                request.FamilyName = SecurityServiceManagerTestData.FamilyName;
                request.EmailAddress = SecurityServiceManagerTestData.EmailAddress;
                request.Password = SecurityServiceManagerTestData.Password;
                request.PhoneNumber = SecurityServiceManagerTestData.PhoneNumber;
                request.Roles = SecurityServiceManagerTestData.Roles;
                request.Claims = SecurityServiceManagerTestData.Claims;

                return request;
            }
        }

        public static String TestDataUserId = "92BEFFCD-CA30-4E71-B8EC-F38E7DD63A25";
        public static String User1Id = "04911949-321d-4a9b-af31-b259160ba94f";
        public static String User2Id = "b8937737-ec2c-488f-bd95-7d053cb1d36e";
        public static String User3Id = "da6ce793-1ed5-4116-af89-2878b075ec5a";

        public static List<IdentityUser> UserList = new List<IdentityUser>
        {
            new IdentityUser
            {
                UserName = "00000001",
                NormalizedUserName = "00000001",
                Email = "00000001@testemail.com",
                NormalizedEmail = "00000001@testemail.com",
                Id = SecurityServiceManagerTestData.User1Id
            },
            new IdentityUser
            {
                UserName = "00000002",
                NormalizedUserName = "00000002",
                Email = "00000002@testemail.com",
                NormalizedEmail = "00000002@testemail.com",
                Id = SecurityServiceManagerTestData.User2Id
            },
            new IdentityUser
            {
                UserName = "00000003",
                NormalizedUserName = "00000003",
                Email = "00000003@testemail.com",
                NormalizedEmail = "00000003@testemail.com",
                Id = SecurityServiceManagerTestData.User3Id
            }
        };

        public static Dictionary<String, List<String>> UserRoles = new Dictionary<String, List<String>>
        {
            {SecurityServiceManagerTestData.User1Id, new List<String>() {"Role1", "Role2"}},
            {SecurityServiceManagerTestData.User2Id, new List<String>() {"Role3"}},
            {SecurityServiceManagerTestData.User3Id, new List<String>() {"Role4"}},
        };

        public static Dictionary<String, List<Claim>> UserClaims = new Dictionary<String, List<Claim>>
        {
            {
                SecurityServiceManagerTestData.User1Id, new List<Claim>()
                {
                    new Claim("ClaimType1", "Value1")
                }
            },
            {
                SecurityServiceManagerTestData.User2Id, new List<Claim>()
                {
                    new Claim("ClaimType1", "Value2")
                }
            },
            {
                SecurityServiceManagerTestData.User3Id, new List<Claim>()
                {
                    new Claim("ClaimType1", "Value3")
                }
            },
        };

        private static String GivenName = "GivenName";

        private static String MiddleName = "MiddleName";
        private static String FamilyName = "FamilyName";
        
        public static IdentityUser IdentityUser = new IdentityUser
                                                  {
            Id = SecurityServiceManagerTestData.TestDataUserId,
            UserName = SecurityServiceManagerTestData.UserName,
            Email = SecurityServiceManagerTestData.EmailAddress,
            PhoneNumber = SecurityServiceManagerTestData.PhoneNumber,
                                                  };

        public async static Task<IQueryable<IdentityUser>> IdentityUsers()
        {

            IQueryable<IdentityUser> result = new List<IdentityUser>
                         {
                             new IdentityUser
                             {
                                 Id = SecurityServiceManagerTestData.User1Id,
                                 UserName = SecurityServiceManagerTestData.UserName1,
                                 Email = SecurityServiceManagerTestData.EmailAddress1,
                                 PhoneNumber = SecurityServiceManagerTestData.PhoneNumber,
                             },
                             new IdentityUser
                             {
                                 Id = SecurityServiceManagerTestData.User2Id,
                                 UserName = SecurityServiceManagerTestData.UserName2,
                                 Email = SecurityServiceManagerTestData.EmailAddress2,
                                 PhoneNumber = SecurityServiceManagerTestData.PhoneNumber,
                             },
                             new IdentityUser
                             {
                                 Id = SecurityServiceManagerTestData.User3Id,
                                 UserName = SecurityServiceManagerTestData.UserName3,
                                 Email = SecurityServiceManagerTestData.EmailAddress3,
                                 PhoneNumber = SecurityServiceManagerTestData.PhoneNumber,
                             }
                         }.AsQueryable();

            return await Task.FromResult(result);
        }
    }
}

    
