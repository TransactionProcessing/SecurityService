namespace SecurityService.Common.Examples
{
    using DataTransferObjects.Requests;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="CreateApiScopeRequest" />
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