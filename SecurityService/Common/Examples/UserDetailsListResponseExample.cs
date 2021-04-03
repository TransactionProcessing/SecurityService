﻿namespace SecurityService.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IExamplesProvider{System.Collections.Generic.List{SecurityService.DataTransferObjects.Responses.UserDetails}}" />
    public class UserDetailsListResponseExample : IExamplesProvider<List<UserDetails>>
    {
        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public List<UserDetails> GetExamples()
        {
            return new List<UserDetails>
                   {
                       new UserDetails
                       {
                           UserId = ExampleData.UserId,
                           Claims = new Dictionary<String, String>
                                    {
                                        { ExampleData.UserClaim1Name, ExampleData.UserClaim1Value },
                                        { ExampleData.UserClaim2Name, ExampleData.UserClaim2Value }
                                    },
                           EmailAddress = ExampleData.UserEmailAddress,
                           PhoneNumber = ExampleData.UserPhoneNumber,
                           Roles = new List<String>
                                   {
                                       ExampleData.RoleName
                                   },
                           UserName = ExampleData.UserName
                       }
                   };
        }
    }
}