namespace SecurityService.Common.Examples
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    [ExcludeFromCodeCoverage]
    public class RoleDetailsListResponseExample : IExamplesProvider<List<RoleDetails>>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public List<RoleDetails> GetExamples()
        {
            return new List<RoleDetails>
                   {
                       new RoleDetails
                       {
                           RoleId = ExampleData.RoleId,
                           RoleName = ExampleData.RoleName
                       }
                   };
        }

        #endregion
    }
}