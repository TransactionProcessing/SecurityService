namespace SecurityService.Common.Examples
{
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
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