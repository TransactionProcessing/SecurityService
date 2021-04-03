namespace SecurityService.Common.Examples
{
    using DataTransferObjects;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IExamplesProvider{SecurityService.DataTransferObjects.CreateClientResponse}" />
    public class CreateClientResponseExample : IExamplesProvider<CreateClientResponse>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public CreateClientResponse GetExamples()
        {
            return new CreateClientResponse
                   {
                       ClientId = ExampleData.ClientId
                   };
        }

        #endregion
    }
}