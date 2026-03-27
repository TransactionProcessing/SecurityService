namespace SecurityService.Common.Examples
{
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class ApiScopeDetailsResponseExample : IExamplesProvider<ApiScopeDetails>
    {
        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public ApiScopeDetails GetExamples()
        {
            return new ApiScopeDetails
                   {
                       DisplayName = ExampleData.ApiScopeDisplayName,
                       Name = ExampleData.ApiScopeName,
                       Description = ExampleData.ApiScopeDescription,
                       Enabled = ExampleData.ApiScopeEnabled
                   };
        }
    }
}