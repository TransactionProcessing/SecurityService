namespace SecurityService.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using DataTransferObjects.Requests;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="CreateClientRequest" />
    public class CreateClientRequestExample : IExamplesProvider<CreateClientRequest>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public CreateClientRequest GetExamples()
        {
            return new CreateClientRequest
                   {
                       Secret = ExampleData.ClientSecret,
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
                       ClientId = ExampleData.ClientId,
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