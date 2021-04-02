namespace SecurityService.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IExamplesProvider{SecurityService.DataTransferObjects.Responses.UserDetails}" />
    public class UserDetailsResponseExample : IExamplesProvider<UserDetails>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public UserDetails GetExamples()
        {
            return new UserDetails
                   {
                       UserId = ExampleData.UserId,
                       Claims = new Dictionary<String, String>
                                {
                                    {ExampleData.UserClaim1Name, ExampleData.UserClaim1Value},
                                    {ExampleData.UserClaim2Name, ExampleData.UserClaim2Value}
                                },
                       EmailAddress = ExampleData.UserEmailAddress,
                       PhoneNumber = ExampleData.UserPhoneNumber,
                       Roles = new List<String>
                               {
                                   ExampleData.RoleName
                               },
                       UserName = ExampleData.UserName
                   };
        }

        #endregion
    }
}