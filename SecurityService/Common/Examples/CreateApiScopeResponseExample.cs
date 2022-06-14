﻿namespace SecurityService.Common.Examples
{
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="CreateApiScopeResponse" />
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