namespace SecurityService.Common.Examples
{
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IExamplesProvider{SecurityService.DataTransferObjects.Responses.CreateRoleResponse}" />
    public class CreateRoleResponseExample : IExamplesProvider<CreateRoleResponse>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public CreateRoleResponse GetExamples()
        {
            return new CreateRoleResponse
                   {
                       RoleId = ExampleData.RoleId
                   };
        }

        #endregion
    }
}