namespace SecurityService.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DataTransferObjects.Requests;
    using Swashbuckle.AspNetCore.Filters;

    [ExcludeFromCodeCoverage]
    public class CreateApiResourceRequestExample : IExamplesProvider<CreateApiResourceRequest>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public CreateApiResourceRequest GetExamples()
        {
            return new CreateApiResourceRequest
                   {
                       Description = ExampleData.ApiResourceDescription,
                       DisplayName = ExampleData.ApiResourceDisplayName,
                       Name = ExampleData.ApiResourceName,
                       Scopes = new List<String>
                                {
                                    ExampleData.ApiResourceScope1,
                                    ExampleData.ApiResourceScope2
                                },
                       Secret = ExampleData.ApiResourceSecret,
                       UserClaims = new List<String>
                                    {
                                        ExampleData.ApiResourceUserClaim1,
                                        ExampleData.ApiResourceUserClaim2
                                    }
                   };
        }

        #endregion
    }
}