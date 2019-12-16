using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.DataTransferObjects.Requests
{
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

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

    [ExcludeFromCodeCoverage]
    /// <summary>
    /// 
    /// </summary>
    public class CreateApiResourceRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [JsonProperty("description")]
        public String Description { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        [JsonProperty("display_name")]
        public String DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        [JsonProperty("scopes")]
        public List<String> Scopes { get; set; }

        /// <summary>
        /// Gets or sets the secret.
        /// </summary>
        /// <value>
        /// The secret.
        /// </value>
        [JsonProperty("secret")]
        public String Secret { get; set; }

        /// <summary>
        /// Gets or sets the user claims.
        /// </summary>
        /// <value>
        /// The user claims.
        /// </value>
        [JsonProperty("user_claims")]
        public List<String> UserClaims { get; set; }

        #endregion
    }
}
