﻿namespace SecurityService.Common.Examples
{
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IExamplesProvider{SecurityService.DataTransferObjects.Responses.CreateIdentityResourceResponse}" />
    public class CreateIdentityResourceResponseExample : IExamplesProvider<CreateIdentityResourceResponse>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public CreateIdentityResourceResponse GetExamples()
        {
            return new CreateIdentityResourceResponse
                   {
                       IdentityResourceName = ExampleData.IdentityResourceName
                   };
        }

        #endregion
    }
}