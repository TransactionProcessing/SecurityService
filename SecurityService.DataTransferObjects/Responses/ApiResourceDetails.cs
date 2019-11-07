namespace SecurityService.DataTransferObjects.Responses
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ApiResourceDetails
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
        /// Gets or sets a value indicating whether this <see cref="ApiResourceDetails"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("enabled")]
        public Boolean Enabled { get; set; }

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