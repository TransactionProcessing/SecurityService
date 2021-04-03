namespace SecurityService.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IExamplesProvider{System.Collections.Generic.List{SecurityService.DataTransferObjects.Responses.ApiResourceDetails}}" />
    public class ApiResourceDetailsListResponseExample : IExamplesProvider<List<ApiResourceDetails>>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public List<ApiResourceDetails> GetExamples()
        {
            return new List<ApiResourceDetails>
                   {
                       new ApiResourceDetails
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
                       }
                   };
        }

        #endregion
    }
}