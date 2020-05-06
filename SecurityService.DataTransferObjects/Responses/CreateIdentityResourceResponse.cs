using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.DataTransferObjects.Responses
{
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    [ExcludeFromCodeCoverage]
    public class CreateIdentityResourceResponse
    {
        /// <summary>
        /// Gets or sets the name of the identity resource.
        /// </summary>
        /// <value>
        /// The name of the identity resource.
        /// </value>
        [JsonProperty("identity_resource_name")]
        public String IdentityResourceName { get; set; }
    }
}
