namespace SecurityService.DataTransferObjects.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    [ExcludeFromCodeCoverage]
    public class ClientDetails
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
        /// Gets or sets a value indicating whether [allow offline access].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow offline access]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("allow_offline_access")]
        public Boolean AllowOfflineAccess { get; set; }

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
        [JsonProperty("client_id")]
        public String ClientId { get; set; }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        [JsonProperty("client_name")]
        public String ClientName { get; set; }

        /// <summary>
        /// Gets or sets the client post logout redirect uris.
        /// </summary>
        /// <value>
        /// The client post logout redirect uris.
        /// </value>
        [JsonProperty("client_post_logout_redirect_uris")]
        public List<String> ClientPostLogoutRedirectUris { get; set; }

        /// <summary>
        /// Gets or sets the client redirect uris.
        /// </summary>
        /// <value>
        /// The client redirect uris.
        /// </value>
        [JsonProperty("client_redirect_uris")]
        public List<String> ClientRedirectUris { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ClientDetails"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("enabled")]
        public Boolean Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [require consent].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require consent]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("require_content")]
        public Boolean RequireConsent { get; set; }

        #endregion
    }
}