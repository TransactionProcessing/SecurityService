namespace SecurityService.Common.Examples
{
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ApiScopeDetails" />
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