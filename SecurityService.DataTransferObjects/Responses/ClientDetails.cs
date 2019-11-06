namespace SecurityService.DataTransferObjects.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class ClientDetails
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ClientDetails"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public Boolean Enabled { get; set; }

        /// <summary>
        /// Gets or sets the allowed grant types.
        /// </summary>
        /// <value>
        /// The allowed grant types.
        /// </value>
        public List<String> AllowedGrantTypes { get; set; }

        /// <summary>
        /// Gets or sets the allowed scopes.
        /// </summary>
        /// <value>
        /// The allowed scopes.
        /// </value>
        public List<String> AllowedScopes { get; set; }

        /// <summary>
        /// Gets or sets the client description.
        /// </summary>
        /// <value>
        /// The client description.
        /// </value>
        public String ClientDescription { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public String ClientId { get; set; }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        public String ClientName { get; set; }

        #endregion
    }
}