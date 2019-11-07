namespace SecurityService.DataTransferObjects.Requests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CreateClientRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the allowed grant types.
        /// </summary>
        /// <value>
        /// The allowed grant types.
        /// </value>
        [JsonProperty("allowed_grant_types")]
        public List<String> AllowedGrantTypes { get; set; }

        /// <summary>
        /// Gets or sets the allowed scopes.
        /// </summary>
        /// <value>
        /// The allowed scopes.
        /// </value>
        [JsonProperty("allowed_scopes")]
        public List<String> AllowedScopes { get; set; }

        /// <summary>
        /// Gets or sets the client description.
        /// </summary>
        /// <value>
        /// The client description.
        /// </value>
        [JsonProperty("client_description")]
        public String ClientDescription { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        [Required]
        [JsonProperty("client_id")]
        public String ClientId { get; set; }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        [Required]
        [JsonProperty("client_name")]
        public String ClientName { get; set; }

        /// <summary>
        /// Gets or sets the secret.
        /// </summary>
        /// <value>
        /// The secret.
        /// </value>
        [Required]
        [JsonProperty("secret")]
        public String Secret { get; set; }

        #endregion
    }
}