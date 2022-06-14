/// <summary>
/// 
/// </summary>
namespace SecurityService.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IdentityResourceDetails" />
    public class IdentityResourceDetailsListResponseExample : IExamplesProvider<List<IdentityResourceDetails>>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public List<IdentityResourceDetails> GetExamples()
        {
            return new List<IdentityResourceDetails>
                   {
                       new IdentityResourceDetails
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
                       }
                   };
        }

        #endregion
    }
}