namespace SecurityService.Common.Examples
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    internal static class ExampleData
    {
        #region Fields

        /// <summary>
        /// The API resource description
        /// </summary>
        public static String ApiResourceDescription = "Example Api Respource";

        /// <summary>
        /// The API resource display name
        /// </summary>
        public static String ApiResourceDisplayName = "Example Api";

        /// <summary>
        /// The API resource enabled
        /// </summary>
        public static Boolean ApiResourceEnabled = true;

        /// <summary>
        /// The API resource name
        /// </summary>
        public static String ApiResourceName = "Example Api";

        /// <summary>
        /// The API resource scope1
        /// </summary>
        public static String ApiResourceScope1 = "ExampleScope1";

        /// <summary>
        /// The API resource scope2
        /// </summary>
        public static String ApiResourceScope2 = "ExampleScope2";

        /// <summary>
        /// The API resource secret
        /// </summary>
        public static String ApiResourceSecret = "ExampleApiSecret";

        /// <summary>
        /// The API resource user claim1
        /// </summary>
        public static String ApiResourceUserClaim1 = "ExampleUserClaim1";

        /// <summary>
        /// The API resource user claim2
        /// </summary>
        public static String ApiResourceUserClaim2 = "ExampleUserClaim2";

        /// <summary>
        /// The API scope description
        /// </summary>
        public static String ApiScopeDescription = "Example Api Scope";

        /// <summary>
        /// The API scope display name
        /// </summary>
        public static String ApiScopeDisplayName = "Example Api Scope Display Name";

        /// <summary>
        /// The API scope enabled
        /// </summary>
        public static Boolean ApiScopeEnabled = true;

        /// <summary>
        /// The API scope name
        /// </summary>
        public static String ApiScopeName = "Example Api Scope Name";

        /// <summary>
        /// The client allowed grant type1
        /// </summary>
        public static String ClientAllowedGrantType1 = "Grant1";

        /// <summary>
        /// The client allowed grant type2
        /// </summary>
        public static String ClientAllowedGrantType2 = "Grant2";

        /// <summary>
        /// The client allowed scope1
        /// </summary>
        public static String ClientAllowedScope1 = "AllowedScope1";

        /// <summary>
        /// The client allowed scope2
        /// </summary>
        public static String ClientAllowedScope2 = "AllowedScope2";

        /// <summary>
        /// The client allow offline access
        /// </summary>
        public static Boolean ClientAllowOfflineAccess = true;

        /// <summary>
        /// The client description
        /// </summary>
        public static String ClientDescription = "Example Client Description";

        /// <summary>
        /// The client enabled
        /// </summary>
        public static Boolean ClientEnabled = true;

        /// <summary>
        /// The client identifier
        /// </summary>
        public static String ClientId = "ExampleClientId";

        /// <summary>
        /// The client name
        /// </summary>
        public static String ClientName = "Example Client";

        /// <summary>
        /// The client post logout redirect URI
        /// </summary>
        public static String ClientPostLogoutRedirectUri = "http://localhost/signout";

        /// <summary>
        /// The client redirect URI
        /// </summary>
        public static String ClientRedirectUri = "http://localhost/signin";

        /// <summary>
        /// The client require consent
        /// </summary>
        public static Boolean ClientRequireConsent = true;

        /// <summary>
        /// The client secret
        /// </summary>
        public static String ClientSecret = "ClientSecret1";

        /// <summary>
        /// The identity resource claim1
        /// </summary>
        public static String IdentityResourceClaim1 = "Example Claim 1";

        /// <summary>
        /// The identity resource claim2
        /// </summary>
        public static String IdentityResourceClaim2 = "Example Claim 2";

        /// <summary>
        /// The identity resource description
        /// </summary>
        public static String IdentityResourceDescription = "Example Identity Resource Description";

        /// <summary>
        /// The identity resource display name
        /// </summary>
        public static String IdentityResourceDisplayName = "Example Identity Resource Display Name";

        /// <summary>
        /// The identity resource emphasize
        /// </summary>
        public static Boolean IdentityResourceEmphasize = true;

        /// <summary>
        /// The identity resource name
        /// </summary>
        public static String IdentityResourceName = "Example Identity Resource";

        /// <summary>
        /// The identity resource required
        /// </summary>
        public static Boolean IdentityResourceRequired = true;

        /// <summary>
        /// The identity resource show in discovery document
        /// </summary>
        public static Boolean IdentityResourceShowInDiscoveryDocument = true;

        /// <summary>
        /// The role identifier
        /// </summary>
        public static Guid RoleId = Guid.Parse("32E13195-A5B5-46B5-A84B-CF63BE06A9EF");

        /// <summary>
        /// The role name
        /// </summary>
        public static String RoleName = "Example Role";

        /// <summary>
        /// The user claim1 name
        /// </summary>
        public static String UserClaim1Name = "ExampleUserClaim1";

        /// <summary>
        /// The user claim1 value
        /// </summary>
        public static String UserClaim1Value = "ExampleUserClaim1Value";

        /// <summary>
        /// The user claim2 name
        /// </summary>
        public static String UserClaim2Name = "ExampleUserClaim2";

        /// <summary>
        /// The user claim2 value
        /// </summary>
        public static String UserClaim2Value = "ExampleUserClaim2Value";

        /// <summary>
        /// The user email address
        /// </summary>
        public static String UserEmailAddress = "exampleuser@exampleuseremail.co.uk";

        /// <summary>
        /// The user family name
        /// </summary>
        public static String UserFamilyName = "Example Family Name";

        /// <summary>
        /// The user given name
        /// </summary>
        public static String UserGivenName = "Example Given Name";

        /// <summary>
        /// The user identifier
        /// </summary>
        public static Guid UserId = Guid.Parse("8A927E15-AC95-421A-B1DC-2EF76AC6096B");

        /// <summary>
        /// The user middle name
        /// </summary>
        public static String UserMiddleName = "Example Middle Name";

        /// <summary>
        /// The user name
        /// </summary>
        public static String UserName = "exampleuser@exampleuseremail.co.uk";

        /// <summary>
        /// The user password
        /// </summary>
        public static String UserPassword = "password";

        /// <summary>
        /// The user phone number
        /// </summary>
        public static String UserPhoneNumber = "1234567890";

        #endregion
    }
}