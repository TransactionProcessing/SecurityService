namespace SecurityService.Common.Examples
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    [ExcludeFromCodeCoverage]
    public class ApiScopeDetailsListResponseExample : IExamplesProvider<List<ApiScopeDetails>>
    {
        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public List<ApiScopeDetails> GetExamples()
        {
            return new List<ApiScopeDetails>
                   {
                       new ApiScopeDetails
                       {
                           DisplayName = ExampleData.ApiScopeDisplayName,
                           Name = ExampleData.ApiScopeName,
                           Description = ExampleData.ApiScopeDescription,
                           Enabled = ExampleData.ApiScopeEnabled
                       }
                   };
        }
    }
}