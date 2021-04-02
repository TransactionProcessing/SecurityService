namespace SecurityService.Common.Examples
{
    using DataTransferObjects.Requests;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IExamplesProvider{SecurityService.DataTransferObjects.Requests.CreateApiScopeRequest}" />
    public class CreateApiScopeRequestExample : IExamplesProvider<CreateApiScopeRequest>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public CreateApiScopeRequest GetExamples()
        {
            return new CreateApiScopeRequest
                   {
                       Description = ExampleData.ApiScopeDescription,
                       Name = ExampleData.ApiScopeName,
                       DisplayName = ExampleData.ApiScopeDisplayName,
                   };
        }

        #endregion
    }
}