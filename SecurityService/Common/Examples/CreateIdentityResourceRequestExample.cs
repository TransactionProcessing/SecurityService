namespace SecurityService.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DataTransferObjects.Requests;
    using Swashbuckle.AspNetCore.Filters;

    [ExcludeFromCodeCoverage]
    public class CreateIdentityResourceRequestExample : IExamplesProvider<CreateIdentityResourceRequest>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public CreateIdentityResourceRequest GetExamples()
        {
            return new CreateIdentityResourceRequest
                   {
                       Description = ExampleData.IdentityResourceDescription,
                       DisplayName = ExampleData.IdentityResourceDisplayName,
                       Claims = new List<String>
                                {
                                    ExampleData.IdentityResourceClaim1,
                                    ExampleData.IdentityResourceClaim2
                                },
                       Emphasize = ExampleData.IdentityResourceEmphasize,
                       Name = ExampleData.IdentityResourceName,
                       Required = ExampleData.IdentityResourceRequired,
                       ShowInDiscoveryDocument = ExampleData.IdentityResourceShowInDiscoveryDocument
                   };
        }

        #endregion
    }
}