namespace SecurityService.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DataTransferObjects;
    using Swashbuckle.AspNetCore.Filters;

    [ExcludeFromCodeCoverage]
    public class CreateUserRequestExample : IExamplesProvider<CreateUserRequest>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public CreateUserRequest GetExamples()
        {
            return new CreateUserRequest
                   {
                       Claims = new Dictionary<String, String>
                                {
                                    {ExampleData.UserClaim1Name, ExampleData.UserClaim1Value},
                                    {ExampleData.UserClaim2Name, ExampleData.UserClaim2Value}
                                },
                       EmailAddress = ExampleData.UserEmailAddress,
                       FamilyName = ExampleData.UserFamilyName,
                       GivenName = ExampleData.UserGivenName,
                       MiddleName = ExampleData.UserMiddleName,
                       PhoneNumber = ExampleData.UserPhoneNumber,
                       ProviderSettings = new Dictionary<String, Object>
                                          {
                                              {
                                                  "keycloak", new Dictionary<String, Object>
                                                               {
                                                                   { "enabled", true },
                                                                   { "email_verified", false },
                                                                   { "required_actions", new List<String> { "VERIFY_EMAIL", "UPDATE_PASSWORD" } },
                                                                   { "groups", new List<String> { "finance", "approvers" } }
                                                               }
                                              }
                                          },
                       Roles = new List<String>
                               {
                                   ExampleData.RoleName
                               }
                   };
        }

        #endregion
    }
}
