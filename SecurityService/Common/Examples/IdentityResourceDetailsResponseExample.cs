namespace SecurityService.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    [ExcludeFromCodeCoverage]
    public class IdentityResourceDetailsResponseExample : IExamplesProvider<IdentityResourceDetails>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public IdentityResourceDetails GetExamples()
        {
            return new IdentityResourceDetails
                   {
                       DisplayName = ExampleData.IdentityResourceDisplayName,
                       Description = ExampleData.IdentityResourceDescription,
                       Name = ExampleData.IdentityResourceName,
                       Claims = new List<String>
                                {
                                    ExampleData.IdentityResourceClaim1,
                                    ExampleData.IdentityResourceClaim2
                                },
                       Emphasize = ExampleData.IdentityResourceEmphasize,
                       Required = ExampleData.IdentityResourceRequired,
                       ShowInDiscoveryDocument = ExampleData.IdentityResourceShowInDiscoveryDocument
                   };
        }

        #endregion
    }
}