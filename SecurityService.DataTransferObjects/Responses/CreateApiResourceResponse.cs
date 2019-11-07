using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.DataTransferObjects.Responses
{
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    [ExcludeFromCodeCoverage]
    public class CreateApiResourceResponse
    {
        /// <summary>
        /// Gets or sets the name of the API resource.
        /// </summary>
        /// <value>
        /// The name of the API resource.
        /// </value>
        [JsonProperty("api_resource_name")]
        public String ApiResourceName { get; set; }
    }
}
