namespace SecurityService.DataTransferObjects.Responses
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    [ExcludeFromCodeCoverage]
    public class CreateApiScopeResponse
    {
        /// <summary>
        /// Gets or sets the name of the API scope.
        /// </summary>
        /// <value>
        /// The name of the API scope.
        /// </value>
        [JsonProperty("api_scope_name")]
        public String ApiScopeName { get; set; }
    }
}