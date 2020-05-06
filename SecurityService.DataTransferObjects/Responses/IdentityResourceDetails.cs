namespace SecurityService.DataTransferObjects.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class IdentityResourceDetails
    {
        #region Properties

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        [JsonProperty("claims")]
        public List<String> Claims { get; set; }

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
        /// Gets or sets a value indicating whether this <see cref="IdentityResourceDetails"/> is emphasize.
        /// </summary>
        /// <value>
        ///   <c>true</c> if emphasize; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("emphasize")]
        public Boolean Emphasize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IdentityResourceDetails"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("required")]
        public Boolean Required { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in discovery document].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in discovery document]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("show_in_discovery_document")]
        public Boolean ShowInDiscoveryDocument { get; set; }

        #endregion
    }
}