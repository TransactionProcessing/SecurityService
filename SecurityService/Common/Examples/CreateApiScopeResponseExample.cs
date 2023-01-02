namespace SecurityService.Common.Examples
{
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class CreateApiScopeResponseExample : IExamplesProvider<CreateApiScopeResponse>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public CreateApiScopeResponse GetExamples()
        {
            return new CreateApiScopeResponse
                   {
                       ApiScopeName = ExampleData.ApiScopeName
                   };
        }

        #endregion
    }
}