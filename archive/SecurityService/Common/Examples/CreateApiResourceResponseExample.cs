namespace SecurityService.Common.Examples
{
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class CreateApiResourceResponseExample : IExamplesProvider<CreateApiResourceResponse>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public CreateApiResourceResponse GetExamples()
        {
            return new CreateApiResourceResponse
                   {
                       ApiResourceName = ExampleData.ApiResourceName
                   };
        }

        #endregion
    }
}