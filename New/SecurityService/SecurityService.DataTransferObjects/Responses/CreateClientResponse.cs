namespace SecurityService.DataTransferObjects.Responses
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    [ExcludeFromCodeCoverage]
    public class CreateClientResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        [JsonProperty("client_id")]
        public String ClientId { get; set; }

        #endregion
    }
}