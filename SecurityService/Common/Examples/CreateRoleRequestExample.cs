﻿namespace SecurityService.Common.Examples
{
    using DataTransferObjects.Requests;
    using Swashbuckle.AspNetCore.Filters;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
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