namespace SecurityService.Common.Examples
{
    using DataTransferObjects.Requests;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IExamplesProvider{SecurityService.DataTransferObjects.Requests.CreateRoleRequest}" />
    public class CreateRoleRequestExample : IExamplesProvider<CreateRoleRequest>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public CreateRoleRequest GetExamples()
        {
            return new CreateRoleRequest
                   {
                       RoleName = ExampleData.RoleName
                   };
        }

        #endregion
    }
}