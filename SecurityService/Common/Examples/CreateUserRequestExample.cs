namespace SecurityService.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using DataTransferObjects;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IExamplesProvider{SecurityService.DataTransferObjects.CreateUserRequest}" />
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
                       Password = ExampleData.UserPassword,
                       PhoneNumber = ExampleData.UserPhoneNumber,
                       Roles = new List<String>
                               {
                                   ExampleData.RoleName
                               }
                   };
        }

        #endregion
    }
}