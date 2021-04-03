namespace SecurityService.Common.Examples
{
    using DataTransferObjects;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IExamplesProvider{SecurityService.DataTransferObjects.CreateUserResponse}" />
    public class CreateUserResponseExample : IExamplesProvider<CreateUserResponse>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public CreateUserResponse GetExamples()
        {
            return new CreateUserResponse
                   {
                       UserId = ExampleData.UserId
                   };
        }

        #endregion
    }
}