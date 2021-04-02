namespace SecurityService.Common.Examples
{
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IExamplesProvider{SecurityService.DataTransferObjects.Responses.RoleDetails}" />
    public class RoleDetailsResponseExample : IExamplesProvider<RoleDetails>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public RoleDetails GetExamples()
        {
            return new RoleDetails
                   {
                       RoleId = ExampleData.RoleId,
                       RoleName = ExampleData.RoleName
                   };
        }

        #endregion
    }
}