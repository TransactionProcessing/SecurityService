namespace SecurityService.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    [ExcludeFromCodeCoverage]
    public class ApiResourceDetailsResponseExample : IExamplesProvider<ApiResourceDetails>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public ApiResourceDetails GetExamples()
        {
            return new ApiResourceDetails
                   {
                       DisplayName = ExampleData.ApiResourceDisplayName,
                       Name = ExampleData.ApiResourceName,
                       Scopes = new List<String>
                                {
                                    ExampleData.ApiResourceScope1,
                                    ExampleData.ApiResourceScope2
                                },
                       UserClaims = new List<String>
                                    {
                                        ExampleData.ApiResourceUserClaim1,
                                        ExampleData.ApiResourceUserClaim2
                                    },
                       Description = ExampleData.ApiResourceDescription,
                       Enabled = ExampleData.ApiResourceEnabled
                   };
        }

        #endregion
    }
}