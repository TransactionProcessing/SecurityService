namespace SecurityService.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using DataTransferObjects.Responses;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IExamplesProvider{SecurityService.DataTransferObjects.Responses.ClientDetails}" />
    public class ClientDetailsResponseExample : IExamplesProvider<ClientDetails>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public ClientDetails GetExamples()
        {
            return new ClientDetails
                   {
                       Enabled = ExampleData.ClientEnabled,
                       ClientId = ExampleData.ClientId,
                       AllowOfflineAccess = ExampleData.ClientAllowOfflineAccess,
                       AllowedGrantTypes = new List<String>
                                           {
                                               ExampleData.ClientAllowedGrantType1,
                                               ExampleData.ClientAllowedGrantType2
                                           },
                       AllowedScopes = new List<String>
                                       {
                                           ExampleData.ClientAllowedScope1,
                                           ExampleData.ClientAllowedScope2
                                       },
                       ClientDescription = ExampleData.ClientDescription,
                       ClientName = ExampleData.ClientName,
                       ClientPostLogoutRedirectUris = new List<String>
                                                      {
                                                          ExampleData.ClientPostLogoutRedirectUri
                                                      },
                       ClientRedirectUris = new List<String>
                                            {
                                                ExampleData.ClientRedirectUri
                                            },
                       RequireConsent = ExampleData.ClientRequireConsent
                   };
        }

        #endregion
    }
}